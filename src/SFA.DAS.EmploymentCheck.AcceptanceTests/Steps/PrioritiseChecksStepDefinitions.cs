using System;
using AutoFixture;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using HMRC.ESFA.Levy.Api.Types;
using Microsoft.Data.SqlClient;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dapper;
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
        private readonly List<Data.Models.EmploymentCheck> _checks;

        public PrioritiseChecksStepDefinitions(TestContext context)
        {
            _context = context;
            _checks = new List<Data.Models.EmploymentCheck>();
        }

        [Given(@"a number of New Pending employment checks are added")]
        public async Task GivenANumberOfNewPendingEmploymentChecksAreAdded()
        {
            await CreateEmploymentChecks(101);
            await CreateEmploymentChecks(102);
        }

        [Given(@"a number of Previously processed employment checks")]
        public async Task GivenANumberOfUnprocessedEmploymentChecks()
        {
            var createdOn = DateTime.UtcNow.AddDays(-1);

            await CreateEmploymentCheckCacheRequest("AB123456A", "Paye1", true, (short?)2, createdOn);
            await CreateEmploymentCheckCacheRequest("AB123456A", "Paye2", false, (short?)2, createdOn.AddDays(-1));

            await CreateEmploymentCheckCacheRequest("CD123456A", "Paye11", true, (short?)2, createdOn.AddDays(-1));
            await CreateEmploymentCheckCacheRequest("CD123456A", "Paye22", false, (short?)2, createdOn);
        }

        private async Task CreateEmploymentChecks(long? apprenticeshipId)
        {
            var check = _context.Fixture.Build<Data.Models.EmploymentCheck>()
                .With(c => c.ApprenticeshipId, apprenticeshipId)
                .With(c => c.RequestCompletionStatus, () => null)
                .With(c => c.Employed, () => null)
                .With(c => c.LastUpdatedOn, () => null)
                .With(c => c.ErrorType, () => null)
                .With(c => c.CreatedOn, DateTime.UtcNow)
                .Create();

            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var id = await dbConnection.InsertAsync(check);
            check.Id = id;
            _checks.Add(check);
        }

        private async Task CreateEmploymentCheckCacheRequest(string nino, string payeScheme, bool employed, short? requestCompletionStatus, DateTime createdOn)
        {
            var request = _context.Fixture.Build<EmploymentCheckCacheRequest>()
                .With(r => r.Nino, nino)
                .With(r => r.Employed, employed)
                .With(r => r.PayeScheme, payeScheme)
                .With(r => r.RequestCompletionStatus, requestCompletionStatus)
                .With(r => r.CreatedOn, createdOn)
                .Without(r => r.Id)
                .Create();

            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);

            await dbConnection.InsertAsync(request);
        }

        [Given(@"valid PAYE schemes are returned for the Accounts")]
        public void GivenPayeSchemesAreReturnedForTheAccounts()
        {
            foreach (var employmentCheck in _checks)
            {
                var accountsApiResponse = employmentCheck.ApprenticeshipId == 101
                    ? new ResourceList(new List<ResourceViewModel> { new ResourceViewModel { Id = "Paye1" }, new ResourceViewModel { Id = "Paye2" } })
                    : new ResourceList(new List<ResourceViewModel> { new ResourceViewModel { Id = "Paye11" }, new ResourceViewModel { Id = "Paye22" } });

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
                        .WithBodyAsJson(accountsApiResponse));
            }
        }

        [Given(@"valid National Insurance Numbers are returned for the learners")]
        public void GivenAValidNationalInsuranceNumbersAreReturnedForTheLearners()
        {
            foreach (var employmentCheck in _checks)
            {
                var dcApiResponse = new List<LearnerNiNumber>
                {
                    new LearnerNiNumber(employmentCheck.Uln, employmentCheck.ApprenticeshipId == 101 ? "AB123456A" : "CD123456A", HttpStatusCode.OK)
                };

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
                        .WithBodyAsJson(dcApiResponse));

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
                    .WithBodyAsJson(_context.Fixture.Create<EmploymentStatus>()));

            await _context.TestFunction.ExecuteCreateEmploymentCheckCacheRequestsOrchestrator();
            await Task.Delay(TimeSpan.FromSeconds(5));
            await _context.TestFunction.ExecuteProcessEmploymentCheckRequestsOrchestrator();
        }

        [Then(@"the Employment Checks Sent to Hmrc order of PayeScheme Priority Order")]
        public async Task ThenTheEmploymentCheckArePerformedInOrderOfSmallestToLargestEmployers()
        {
            var correlationIds = _checks.Select(c => c.CorrelationId).ToArray();

            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var result = (await dbConnection.QueryAsync<EmploymentCheckCacheRequest>(
                sql: @"SELECT * FROM [Cache].[EmploymentCheckCacheRequest] WHERE CorrelationId in @CorrelationIds",
                param: new { CorrelationIds = correlationIds },
                commandType: CommandType.Text)).ToList();

            result.Count.Should().Be(4);

            result.SingleOrDefault(r => r.PayeScheme == "Paye1")?.RequestCompletionStatus.Should().Be(2);
            result.SingleOrDefault(r => r.PayeScheme == "Paye2")?.RequestCompletionStatus.Should().Be(3);
            result.SingleOrDefault(r => r.PayeScheme == "Paye11")?.RequestCompletionStatus.Should().Be(2);
            result.SingleOrDefault(r => r.PayeScheme == "Paye22")?.RequestCompletionStatus.Should().Be(3);
        }

        [Then(@"the Employment Checks are performed in order of PayeScheme Priority Order")]
        public async Task ThenTheEmploymentChecksArePerformedInOrderOfFirstToLastReceived()
        {
            var correlationIds = _checks.Select(c => c.CorrelationId);

            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var result = (await dbConnection.QueryAsync<EmploymentCheckCacheRequest>(
                sql: @"SELECT * FROM [Cache].[EmploymentCheckCacheRequest] WHERE CorrelationId in @CorrelationIds",
                param: new { CorrelationIds = correlationIds },
                commandType: CommandType.Text)).ToList();

            result.Count.Should().Be(4);

            result.SingleOrDefault(r => r.PayeScheme == "Paye1")?.PayeSchemePriority.Should().Be(1);
            result.SingleOrDefault(r => r.PayeScheme == "Paye2")?.PayeSchemePriority.Should().Be(2);
            result.SingleOrDefault(r => r.PayeScheme == "Paye11")?.PayeSchemePriority.Should().Be(1);
            result.SingleOrDefault(r => r.PayeScheme == "Paye22")?.PayeSchemePriority.Should().Be(2);
        }
    }
}
