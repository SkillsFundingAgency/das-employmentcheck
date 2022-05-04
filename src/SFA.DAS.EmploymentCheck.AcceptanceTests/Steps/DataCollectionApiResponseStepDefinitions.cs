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
    [Scope(Feature = "DataCollectionApiResponse")]
    public class DataCollectionApiResponseStepDefinitions
    {

        private Data.Models.EmploymentCheck _checkCacheRequest;
        private List<LearnerNiNumber> _dcApiResponse;
        private Data.Models.EmploymentCheck _check;
        private readonly TestContext _context;

        public DataCollectionApiResponseStepDefinitions(TestContext context)
        {
            _context = context;
        }

        [Given(@"an existing employment check cache request")]
        public async Task GivenAnExistingEmploymentCheckCacheRequest()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            _check = _context.Fixture.Build<Data.Models.EmploymentCheck>()
                .Without(c => c.RequestCompletionStatus)
                .Without(c => c.Employed)
                .Without(c => c.LastUpdatedOn)
                .Without(c => c.ErrorType)
                .Create();

            await dbConnection.InsertAsync(_check);
        }

        [When(@"DataCollection Api call returns (.*) status code")]
        public async Task WhenDataCollectionApiCallReturnsStatusCode(int statusCode)
        {
            HttpStatusCode httpStatusCode = (HttpStatusCode)Enum.ToObject(typeof(HttpStatusCode), statusCode);

            _dcApiResponse = new List<LearnerNiNumber>
                { new LearnerNiNumber(_check.Uln, _context.Fixture.Create<string>()[..10], httpStatusCode)};

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
                    .WithStatusCode(statusCode)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_dcApiResponse));

            var response = await _context.TestFunction.Start(
                new OrchestrationStarterInfo(
                    nameof(CreateEmploymentCheckRequestsOrchestratorHttpTrigger),
                    args: new Dictionary<string, object>
                    {
                        ["req"] = new DummyHttpRequest { Path = "/api/orchestrators/CreateEmploymentCheckRequestsOrchestrator" }
                    },
                    orchestrationName: nameof(CreateEmploymentCheckCacheRequestsOrchestrator)
                ));

            response.StatusCode.Should().NotBe(HttpStatusCode.Conflict,
                "A running instance of the orchestrator detected. Manually delete it and retry.");
        }

        [Then(@"the Api call with (.*) is retried (.*) times")]
        public void ThenTheApiCallWithIsRetriedTimes(int statusCode, int noOfRetries)
        {
            var allLogs = _context.DataCollectionsApi.MockServer.LogEntries.ToList();

            var logs = _context.DataCollectionsApi.MockServer.LogEntries
               .Where(l => (int)l.ResponseMessage.StatusCode == statusCode)
               .ToList();

            logs.Should().HaveCount(noOfRetries);
        }


        
    }
}
