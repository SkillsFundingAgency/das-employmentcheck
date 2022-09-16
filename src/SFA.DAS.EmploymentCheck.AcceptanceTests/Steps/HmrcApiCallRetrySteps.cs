using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.TestHelpers.AzureDurableFunctions;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using TechTalk.SpecFlow;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "HmrcApiCallRetry")]
    public class HmrcApiCallRetrySteps
    {
        private EmploymentCheckCacheRequest _checkCacheRequest;
        private readonly TestContext _context;
        private Data.Models.EmploymentCheck _check;

        public HmrcApiCallRetrySteps(TestContext context)
        {
            _context = context;
        }

        [Given(@"an existing employment check cache request")]
        public async Task GivenAnExistingEmploymentCheckCacheRequest()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
           
            _check = _context.Fixture.Build<Data.Models.EmploymentCheck>()
                .With(c => c.RequestCompletionStatus, (short?) 1)
                .Without(c => c.Employed)
                .Without(c => c.LastUpdatedOn)
                .Create();

            var checkId = await dbConnection.InsertAsync(_check);

            _checkCacheRequest = _context.Fixture.Build<Data.Models.EmploymentCheckCacheRequest>()
                .With(c => c.ApprenticeEmploymentCheckId, checkId)
                .Without(c => c.RequestCompletionStatus)
                .Without(c => c.Employed)
                .Without(c => c.LastUpdatedOn)
                .Create();

            await dbConnection.InsertAsync(_checkCacheRequest);
        }

        [When(@"Hmrc Api call returns (.*) status code")]
        public async Task WhenHmrcApiCallReturnsStatusCode(int statusCode)
        {
            var url = $"/apprenticeship-levy/epaye/{_checkCacheRequest.PayeScheme}/employed/{_checkCacheRequest.Nino}";

            _context.HmrcApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath(url)
                        .WithParam("fromDate", new ExactMatcher($"{_checkCacheRequest.MinDate:yyyy-MM-dd}"))
                        .WithParam("toDate", new ExactMatcher($"{_checkCacheRequest.MaxDate:yyyy-MM-dd}"))
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(statusCode)
                );

            var response = await _context.TestFunction.Start(
                new OrchestrationStarterInfo(
                    nameof(ProcessEmploymentChecksOrchestratorHttpTrigger),
                    args: new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest { Path = "/api/orchestrators/ProcessApprenticeEmploymentChecksOrchestrator" }
                    },
                    orchestrationName: nameof(ProcessEmploymentCheckRequestsOrchestrator)
                ));

            response.StatusCode.Should().NotBe(HttpStatusCode.Conflict,
                "A running instance of the orchestrator detected. Manually delete it and retry.");
        }

        [Then(@"the Api call with (.*) is retried (.*) times")]
        public void ThenTheApiCallWithIsRetriedTimes(int statusCode, int noOfRetries)
        {
            var logs = _context.HmrcApi.MockServer.LogEntries
               .Where(l => (int)l.ResponseMessage.StatusCode == statusCode)
               .ToList();

            logs.Should().HaveCount(noOfRetries + 1);

            
        }

    }
}
