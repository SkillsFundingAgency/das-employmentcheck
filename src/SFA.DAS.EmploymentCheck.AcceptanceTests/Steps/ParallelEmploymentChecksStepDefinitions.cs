using System;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using HMRC.ESFA.Levy.Api.Types;
using Microsoft.Data.SqlClient;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Data.Models;
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
    [Scope(Feature = "EmploymentChecksArePerformedInParallel")]
    public class ParallelEmploymentChecksStepDefinitions
    {
        private readonly TestContext _context;
        private readonly List<Data.Models.EmploymentCheck> _checks;
        private List<LearnerNiNumber> _dcApiResponse;
        private ResourceList _accountsApiResponse;

        public ParallelEmploymentChecksStepDefinitions(TestContext context)
        {
            _context = context;
            _checks = new List<Data.Models.EmploymentCheck>();
        }
        
        [Given(@"a number of unprocessed employment checks")]
        public async Task GivenANumberOfUnprocessedEmploymentChecks()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            for (var i = 0; i < 10; i++)
            {
                var check = _context.Fixture.Build<Data.Models.EmploymentCheck>()
                    .Without(c => c.RequestCompletionStatus)
                    .Without(c => c.Employed)
                    .Without(c => c.LastUpdatedOn)
                    .Without(c => c.ErrorType)
                    .With(c => c.CreatedOn, DateTime.UtcNow.AddMinutes(i-5))
                    .Create();

                await dbConnection.InsertAsync(check);
                _checks.Add(check);
            }
        }

        [Given(@"valid PAYE schemes are returned for the Accounts")]
        public void GivenPayeSchemesAreReturnedForTheAccounts()
        {
            for (var i = 0; i < 10; i++)
            {
                SetupAccountsApiMock(_checks[i], 1);
            }
        }


        private void SetupAccountsApiMock(Data.Models.EmploymentCheck employmentCheck, int noOfPayeSchemes)
        {
            _accountsApiResponse = new ResourceList(_context.Fixture.CreateMany<ResourceViewModel>(noOfPayeSchemes));

            var url = $"/api/accounts/{_context.HashingService.HashValue(employmentCheck.AccountId)}/payeschemes";

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
                _dcApiResponse = new List<LearnerNiNumber> { new LearnerNiNumber(employmentCheck.Uln, _context.Fixture.Create<string>()[..10], HttpStatusCode.OK)};

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

                _context.DataCollectionsApi.MockServer
                  .Given(
                      Request
                          .Create()
                          .WithPath("/api/v1/academic-years")
                          .UsingGet()
                  )
                  .RespondWith(Response.Create()
                      .WithStatusCode(HttpStatusCode.OK)
                      .WithHeader("Content-Type", "application/json")
                      .WithBody("[2122]"));
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
                    .WithBodyAsJson(_context.Fixture.Create<EmploymentStatus>())
                    );

            await _context.TestFunction.ExecuteCreateEmploymentCheckCacheRequestsOrchestrator();
            await Task.Delay(TimeSpan.FromSeconds(30));
            await _context.TestFunction.ExecuteProcessEmploymentCheckRequestsOrchestrator();
        }


        //NOTE: Make sure your Azure table storage has HmrcApiRateLimiterOptions.EmploymentCheckBatchSize > 1, default is 2 
        [Then(@"the Employment Checks are performed in Parallel")]
        public async Task ThenTheEmploymentCheckArePerformedInParallel()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);

            var result = (await dbConnection.GetAllAsync<Data.Models.EmploymentCheck>()).ToList();
            
            result.Any(r => r.RequestCompletionStatus == null || r.RequestCompletionStatus == 1).Should().BeFalse();

            var parallelRunsPerSecond = result.GroupBy(g => g.LastUpdatedOn.Value.ToString("yy-MM-dd-hh-mm-ss"))
                .Select(g => new { g.Key, ExecutionCount = g.Count() })
                .Distinct();

            parallelRunsPerSecond.Any(pr => pr.ExecutionCount > 1).Should().BeTrue();
        }
    }
}
