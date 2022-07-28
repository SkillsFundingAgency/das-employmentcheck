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
using SFA.DAS.Encoding;
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
        private List<LearnerNiNumber> _dcApiResponse;
        private ResourceList _accountsApiResponse;
        private bool _samePayeVolume;

        public PrioritiseChecksStepDefinitions(TestContext context)
        {
            _context = context;
            _checks = new List<Data.Models.EmploymentCheck>();
        }

        [Given(@"a number of unprocessed employment checks")]
        public async Task GivenANumberOfUnprocessedEmploymentChecks()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            for (var i = 0; i < 3; i++)
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

        [Given(@"all employers have the same number of PAYE schemes")]
        public void GivenAllEmployersHaveTheSameNumberOfPayeSchemes()
        {
            _samePayeVolume = true;
        }

        [Given(@"valid PAYE schemes are returned for the Accounts")]
        public void GivenPayeSchemesAreReturnedForTheAccounts()
        {
            SetupAccountsApiMock(_checks[0], _samePayeVolume ? 2 : 3);
            SetupAccountsApiMock(_checks[1], 2);
            SetupAccountsApiMock(_checks[2], _samePayeVolume ? 2 : 1);
        }

        private long[] LowestToHighestNumberOfPayeSchemes => new [] { _checks[2].Id, _checks[1].Id, _checks[0].Id };
        private long[] FirstToLastReceived => new [] { _checks[0].Id, _checks[1].Id, _checks[2].Id };

        private void SetupAccountsApiMock(Data.Models.EmploymentCheck employmentCheck, int noOfPayeSchemes)
        {
            _accountsApiResponse = new ResourceList(_context.Fixture.CreateMany<ResourceViewModel>(noOfPayeSchemes));

            var url =
                $"/api/accounts/{_context.EncodingService.Encode(employmentCheck.AccountId, EncodingType.AccountId)}/payeschemes";

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
                .Select(r => r.Id)
                .Distinct();

            result.Should().BeEquivalentTo(LowestToHighestNumberOfPayeSchemes, options => options.WithStrictOrdering());
        }

        [Then(@"the Employment Checks are performed in order of first to last received")]
        public async Task ThenTheEmploymentChecksArePerformedInOrderOfFirstToLastReceived()
        {
            await using var dbConnection = new SqlConnection(_context.SqlDatabase.DatabaseInfo.ConnectionString);
            var result = (await dbConnection.GetAllAsync<Data.Models.EmploymentCheck>())
                .OrderBy(r => r.LastUpdatedOn)
                .Select(r => r.Id)
                .Distinct();

            result.Should().BeEquivalentTo(FirstToLastReceived, options => options.WithStrictOrdering());
        }
    }
}
