using AutoFixture;
using Dapper.Contrib.Extensions;
using HMRC.ESFA.Levy.Api.Types;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Data.Models;
using System;
using System.Collections.Generic;
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
    [Scope(Feature = "Compliance")]
    public class ComplianceSteps
    {
        private readonly TestContext _context;
        private Data.Models.EmploymentCheck _check;
        private List<LearnerNiNumber> _dcApiResponse;
        private ResourceList _accountsApiResponse;

        public ComplianceSteps(TestContext context)
        {
            _context = context;
        }

        [Given(@"an unprocessed employment check")]
        public async Task GivenAnUnprocessedEmploymentCheck()
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

        [Given(@"a valid PAYE scheme is returned for the Account")]
        public void GivenAValidPayeSchemeIsReturnedForTheAccount()
        {
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
                    .WithStatusCode(HttpStatusCode.OK)
                    .WithHeader("Content-Type", "application/json")
                    .WithBodyAsJson(_accountsApiResponse));
        }

        [Given(@"a valid National Insurance Number returned for the learner")]
        public void GivenAValidNationalInsuranceNumberReturnedForTheLearner()
        {
            _dcApiResponse = new List<LearnerNiNumber>
                { new LearnerNiNumber(_check.Uln, _context.Fixture.Create<string>()[..10], HttpStatusCode.OK)};

            string path =  $"{_context.DataCollectionsApiConfiguration.Path}/2122";

            _context.DataCollectionsApi.MockServer
                .Given(
                    Request
                        .Create()
                        .WithPath(path)
                        .WithParam("ulns", new ExactMatcher($"{_check.Uln}"))
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
                      .WithPath(_context.DataCollectionsApiConfiguration.AcademicYearsPath)
                      .UsingGet()
              )
              .RespondWith(Response.Create()
                  .WithStatusCode(HttpStatusCode.OK)
                  .WithHeader("Content-Type", "application/json")
                  .WithBody("[2122, 2021, 2223]"));
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

            await _context.TestFunction.ExecuteCreateEmploymentCheckCacheRequestsOrchestrator();
            await _context.TestFunction.ExecuteProcessEmploymentCheckRequestsOrchestrator();
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
