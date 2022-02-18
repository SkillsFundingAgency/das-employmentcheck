using AutoFixture;
using Dapper.Contrib.Extensions;
using HMRC.ESFA.Levy.Api.Types;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using SFA.DAS.EmploymentCheck.AcceptanceTests.AzureDurableFunctions;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using TechTalk.SpecFlow;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
   // [Scope(Feature = "Compliance")]
    public class ComplianceSteps
    {
        private readonly TestContext _context;
        private Data.Models.EmploymentCheck _check;
        private LearnerNiNumber[] _dcApiResponse;
        private ResourceList _accountsApiResponse;

        public ComplianceSteps(TestContext context)
        {
            _context = context;
        }

        [Given(@"an unprocessed employment check for AccountId (.*) and ULN number (.*)")]
        public async Task GivenAnUnprocessedEmploymentCheckForAccountIdAndULNNumber(int accountId, int uln)
        {
            _check = _context.Fixture.Build<Data.Models.EmploymentCheck>()
                .With(c => c.AccountId, accountId)
                .With(c => c.Uln, uln)
                .Without(c => c.RequestCompletionStatus)
                .Without(c => c.Employed)
                .Without(c => c.LastUpdatedOn)
                .Create();

            //  await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            //await dbConnection.InsertAsync(_check);
        }

        [Given(@"a valid PAYE scheme is returned for the Account")]
        public void GivenAValidPAYESchemeIsReturnedForTheAccount()
        {
            _accountsApiResponse = _context.Fixture.Create<ResourceList>();

            _context.EmployerAccountsApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"api/accounts/{_context.HashingService.HashValue(_check.AccountId)}/payeschemes")
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
            _dcApiResponse = new[] { new LearnerNiNumber(_check.Uln, _context.Fixture.Create<string>()) };

            _context.EmployerAccountsApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath($"/api/v1/ilr-data/learnersNi/2122?ulns={_check.Uln}")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_dcApiResponse));
        }
        
        [When(@"the Employment Check is performed")]
        public void WhenTheEmploymentCheckIsPerformed()
        {
            var hmrcApiResponse = new EmploymentStatus
            {
                Employed = true
            };

            var url =
                $"/apprenticeship-levy/epaye/{_accountsApiResponse[0].Href}/employed/" +
                $"{_dcApiResponse[0].NiNumber}?" +
                $"fromDate={_check.MinDate:yyyy-MM-dd}&" +
                $"toDate={_check.MaxDate:yyyy-MM-dd}";

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
                    .WithBodyAsJson(hmrcApiResponse));
            //CreateEmploymentCheckRequestsOrchestratorHttpTrigger

            _context.TestFunction.Start(
                new OrchestrationStarterInfo(
                    "CreateEmploymentCheckRequestsOrchestratorHttpTrigger",
                    nameof(CreateEmploymentCheckCacheRequestsOrchestrator),
                    new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest
                        {
                            Path = "/api/orchestrators/CreateEmploymentCheckRequestsOrchestrator"
                        }
                    }
                ));


            //_context.TestFunction.Start(
            //    new OrchestrationStarterInfo(
            //        "EmploymentChecksHttpTrigger",
            //        nameof(EmploymentChecksOrchestrator),
            //        new Dictionary<string, object>
            //        {
            //            ["req"] = new DummyHttpRequest
            //            {
            //                Path = "/api/orchestrators/EmploymentChecksOrchestrator"
            //            }
            //        }
            //    ));
        }
        
        [Then(@"the Employment Check result is stored")]
        public async Task ThenTheEmploymentCheckResultIsStored()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var result = await dbConnection.GetAsync<Data.Models.EmploymentCheck>(_check.Id);

            Assert.IsTrue(result.Employed);
        }
    }
    
}
