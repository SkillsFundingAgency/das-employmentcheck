using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using HMRC.ESFA.Levy.Api.Types;
using Microsoft.Data.SqlClient;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "PrioritiseChecks")]
    public class PrioritiseChecksStepDefinitions
    {
        private readonly TestContext _context;
        private List<Data.Models.EmploymentCheck> _checks;
        private List<LearnerNiNumber> _dcApiResponse;
        private ResourceList _accountsApiResponse;
        private Guid[] _expectedProcessingOrder;

        public PrioritiseChecksStepDefinitions(TestContext context)
        {
            _context = context;
        }

        [Given(@"a number of unprocessed employment checks")]
        public async Task GivenANumberOfUnprocessedEmploymentChecks()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            _checks = _context.Fixture.Build<Data.Models.EmploymentCheck>()
                .Without(c => c.RequestCompletionStatus)
                .Without(c => c.Employed)
                .Without(c => c.LastUpdatedOn)
                .Without(c => c.ErrorType)
                .CreateMany(3).ToList();

            foreach (var check in _checks)
            {
                await dbConnection.InsertAsync(check);
            }
        }

        [Given(@"valid PAYE schemes are returned for the Accounts")]
        public void GivenPayeSchemesAreReturnedForTheAccounts()
        {
            SetupAccountsApiMock(_checks[0], 3);
            SetupAccountsApiMock(_checks[1], 2);
            SetupAccountsApiMock(_checks[2], 1);

            _expectedProcessingOrder = new [] { _checks[2].CorrelationId, _checks[1].CorrelationId, _checks[0].CorrelationId };
        }

        private void SetupAccountsApiMock(Data.Models.EmploymentCheck employmentCheck, int noOfPayeSchemes)
        {
            _accountsApiResponse = new ResourceList(_context.Fixture.CreateMany<ResourceViewModel>(noOfPayeSchemes));

            var url =
                $"/api/accounts/{_context.HashingService.HashValue(employmentCheck.AccountId)}/payeschemes";

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

        [Given(@"valid National Insurance Numbers are returned for the learners")]
        public void GivenAValidNationalInsuranceNumbersAreReturnedForTheLearners()
        {
            foreach (var employmentCheck in _checks)
            {
                _dcApiResponse = new List<LearnerNiNumber>
                    { new LearnerNiNumber(employmentCheck.Uln, _context.Fixture.Create<string>()[..10], HttpStatusCode.OK)};

                const string url = "/api/v1/ilr-data/learnersNi/2122";

                _context.DataCollectionsApi.MockServer
                    .Given(
                        Request
                            .Create()
                            .WithPath(url)
                            .WithParam("ulns", new ExactMatcher($"{employmentCheck.Uln}"))
                            .UsingGet()
                    )
                    .RespondWith(Response.Create()
                        .WithStatusCode(HttpStatusCode.OK)
                        .WithHeader("Content-Type", "application/json")
                        .WithBodyAsJson(_dcApiResponse));
            }
        }

        [When(@"the Employment Checks are performed")]
        public async Task WhenTheEmploymentCheckArePerformed()
        {
            _context.HmrcApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath("*")
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_context.Fixture.Create<EmploymentStatus>()));

            await _context.TestFunction.ExecuteCreateEmploymentCheckCacheRequestsOrchestrator();
            await _context.TestFunction.ExecuteProcessEmploymentCheckRequestsOrchestrator();
        }

        [Then(@"the Employment Checks are performed in order of smallest to largest employers")]
        public async Task ThenTheEmploymentCheckArePerformedInOrderOfSmallestToLargestEmployers()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var result = (await dbConnection.GetAllAsync<Data.Models.EmploymentCheck>())
                .OrderBy(r => r.LastUpdatedOn)
                .Select(r => r.CorrelationId)
                .Distinct();

            result.Should().BeEquivalentTo(_expectedProcessingOrder, options => options.WithStrictOrdering());
        }
    }
}
