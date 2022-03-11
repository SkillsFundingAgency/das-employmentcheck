using AutoFixture;
using Dapper.Contrib.Extensions;
using HMRC.ESFA.Levy.Api.Types;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Tests.AzureFunctions.AzureDurableFunctions;
using TechTalk.SpecFlow;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "Compliance")]
    public class ComplianceSteps
    {
        private readonly TestContext _context;
        private Data.Models.EmploymentCheck _check;
        private List<LearnerNiNumber> _dcApiResponse;
        private ResourceList _accountsApiResponse;
        private const int MaxWaitTimeInSeconds = 30;

        public ComplianceSteps(TestContext context)
        {
            _context = context;
        }

        [Given(@"an unprocessed employment check for AccountId (.*) and ULN number (.*)")]
        public async Task GivenAnUnprocessedEmploymentCheckForAccountIdAndUlnNumber(int accountId, int uln)
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            _check = _context.Fixture.Build<Data.Models.EmploymentCheck>()
                .With(c => c.AccountId, accountId)
                .With(c => c.Uln, uln)
                .Without(c => c.RequestCompletionStatus)
                .Without(c => c.Employed)
                .Without(c => c.LastUpdatedOn)
                .Create();
            
            await dbConnection.InsertAsync(_check);
        }

        [Given(@"a valid PAYE scheme is returned for the Account")]
        public void GivenAValidPayeSchemeIsReturnedForTheAccount()
        {
            _accountsApiResponse = new ResourceList(_context.Fixture.CreateMany<ResourceViewModel>(1));

            var url = $"/api/accounts/{_context.HashingService.HashValue(_check.AccountId)}/payeschemes";

            _context.EmployerAccountsApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath(url)
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_accountsApiResponse));
        }

        [Given(@"a valid National Insurance Number returned for the learner")]
        public void GivenAValidNationalInsuranceNumberReturnedForTheLearner()
        {
            _dcApiResponse = new List<LearnerNiNumber>
                { new LearnerNiNumber(_check.Uln, _context.Fixture.Create<string>()[..10]) };

            const string url = "/api/v1/ilr-data/learnersNi/2122";

            _context.DataCollectionsApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath(url)
                        .WithParam("ulns", new ExactMatcher($"{_check.Uln}"))
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_dcApiResponse));
        }
        
        [When(@"the Employment Check is performed")]
        public async Task WhenTheEmploymentCheckIsPerformed()
        {
            var hmrcApiResponse = new EmploymentStatus { Employed = true };

            var url = $"/apprenticeship-levy/epaye/{_accountsApiResponse[0].Id.ToUpper()}/employed/{_dcApiResponse[0].NiNumber}";

            _context.HmrcApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath(url)
                        .WithParam("fromDate", new ExactMatcher($"{_check.MinDate:yyyy-MM-dd}"))
                        .WithParam("toDate", new ExactMatcher($"{_check.MaxDate:yyyy-MM-dd}"))
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(hmrcApiResponse));

            await _context.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "EmploymentChecksHttpTrigger",
                    nameof(ProcessEmploymentCheckRequestsOrchestrator),
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest { Path = "/api/orchestrators/EmploymentChecksOrchestrator" }
                    },
                    expectedCustomStatus: "Idle"
                ));
        }

        [Then(@"the Employment Check result is stored")]
        public async Task ThenTheEmploymentCheckResultIsStored()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);

            var attempt = 0;
            while (attempt < MaxWaitTimeInSeconds)
            {
                attempt++;
                var result = await dbConnection.GetAsync<Data.Models.EmploymentCheck>(_check.Id);
                if (result.Employed.HasValue)
                {
                    Assert.IsTrue(result.Employed);
                    break;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }

            if (attempt == MaxWaitTimeInSeconds)
            {
                Assert.Inconclusive("Timed out, try increasing MaxWaitTimeInSeconds");
            }
        }
    }
    
}
