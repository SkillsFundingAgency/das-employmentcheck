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
using System;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    public class HmrcApiCallRetryTimeSteps
    {
        private EmploymentCheckCacheRequest _checkCacheRequest;
        private readonly TestContext _context;
        private int _statusCode = 0;
        private DateTime _dtStart = DateTime.MinValue;
        private DateTime _dtEnd = DateTime.MaxValue;

        public HmrcApiCallRetryTimeSteps(TestContext context)
        {
            _context = context;
        }

        [Given(@"an existing employment check cache request")]
        public async Task GivenAnExistingEmploymentCheckCacheRequest()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            _checkCacheRequest = _context.Fixture.Build<Data.Models.EmploymentCheckCacheRequest>()
                .Without(c => c.RequestCompletionStatus)
                .Without(c => c.Employed)
                .Without(c => c.LastUpdatedOn)
                .Create();

            await dbConnection.InsertAsync(_checkCacheRequest);
        }

        [When(@"Hmrc Api call returns (.*) status code")]
        public async Task WhenHmrcApiCallReturnsStatusCode(int statusCode)
        {
            _dtStart = DateTime.Now;

            _statusCode = statusCode;

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

        [Then(@"then the Api has done (.*) retries within (.*) seconds")]
        public void ThenThenTheApiHasDoneRetriesWithinSeconds(int retries, int withInTimeInSeconds)
        {
            _dtEnd = DateTime.Now;

            var logs = _context.HmrcApi.MockServer.LogEntries
              .Where(l => (int)l.ResponseMessage.StatusCode == _statusCode)
              .ToList();

            TimeSpan ts = _dtEnd - _dtStart;

            logs.Should().HaveCount(retries + 1);
            ts.Seconds.Should().BeLessThan(withInTimeInSeconds);
        }
    }
}
