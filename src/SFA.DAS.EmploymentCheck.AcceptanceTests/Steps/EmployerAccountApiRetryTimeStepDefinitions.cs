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
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Steps
{
    [Binding]
    [Scope(Feature = "EmployerAccountApiRetryTime")]
    public class EmployerAccountApiRetryTimeStepDefinitions
    {
        private List<LearnerNiNumber> _dcApiResponse;
        private Data.Models.EmploymentCheck _check;
        private ResourceList _accountsApiResponse;
        private readonly TestContext _context;
        private int _statusCode = 0;
        private DateTime _dtStart = DateTime.MinValue;
        private DateTime _dtEnd = DateTime.MaxValue;

        public EmployerAccountApiRetryTimeStepDefinitions(TestContext context)
        {
            _context = context;
        }

        [Given(@"an employment check cache request")]
        public async Task GivenAnEmploymentCheckCacheRequest()
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

        [When(@"Accoints Api call returns (.*) status code")]
        public async Task WhenAccointsApiCallReturnsStatusCode(int statusCode)
        {
            _dtStart = DateTime.Now;
            _statusCode = statusCode;

            HttpStatusCode httpStatusCode = (HttpStatusCode)Enum.ToObject(typeof(HttpStatusCode), statusCode);


            _dcApiResponse = new List<LearnerNiNumber>
                { new LearnerNiNumber(_check.Uln, _context.Fixture.Create<string>()[..10], httpStatusCode)};

            const string datacollectionsurl = "/api/v1/ilr-data/learnersNi/2122";

            _context.DataCollectionsApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath(datacollectionsurl)
                        .WithParam("ulns", new ExactMatcher($"{_check.Uln}"))
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(statusCode)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_dcApiResponse));


            _accountsApiResponse = new ResourceList(_context.Fixture.CreateMany<ResourceViewModel>(1));

            var url = $"/api/accounts/{_context.EncodingService.Encode(_check.AccountId, EncodingType.AccountId)}/payeschemes";

            _context.EmployerAccountsApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath(url)
                        .UsingGet()
                )
                .RespondWith(Response.Create()
                    .WithStatusCode(statusCode)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_accountsApiResponse));

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

        [Then(@"then the Api has done (.*) retries within (.*) seconds")]
        public void ThenThenTheApiHasDoneRetriesWithinSeconds(int noOfRetries, int withInTimeInSeconds)
        {
            _dtEnd = DateTime.Now;

            var logs = _context.EmployerAccountsApi.MockServer.LogEntries
               .Where(l => (int)l.ResponseMessage.StatusCode == _statusCode)
               .ToList();

            TimeSpan ts = _dtEnd - _dtStart;

            logs.Should().HaveCount(noOfRetries + 1);
            ts.Seconds.Should().BeLessThan(withInTimeInSeconds);

            
        }
    }
}
