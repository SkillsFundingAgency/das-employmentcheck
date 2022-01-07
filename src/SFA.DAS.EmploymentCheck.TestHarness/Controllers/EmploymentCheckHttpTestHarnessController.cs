using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.TokenServiceStub;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TestHarness.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmploymentCheckHttpTestHarnessController : ControllerBase
    {
        private readonly IHmrcAuthTokenBroker _authTokenBroker;
        private readonly ILogger<EmploymentCheckHttpTestHarnessController> _logger;

        public EmploymentCheckHttpTestHarnessController(ILogger<EmploymentCheckHttpTestHarnessController> logger,
            IHmrcAuthTokenBroker authTokenBroker)
        {
            _logger = logger;
            _authTokenBroker = authTokenBroker;
        }

        /// <summary>
        /// EmploymentCheckHttpTestHarness for single employee check
        /// https://skillsfundingagency.atlassian.net/browse/HTB-17
        /// </summary>
        /// <param name="nino">national insurance number</param>
        /// <param name="empRef">employer reference</param>
        /// <param name="fromDate">from date</param>
        /// <param name="toDate">to date</param>
        /// <returns><see cref="EmploymentStatus"/> object</returns>
        /// sample request: https://localhost:5001/EmploymentCheckHttpTestHarness?nino=PR555555A&empRef=923%2FEZ00059&fromDate=2010-01-01&toDate=2018-01-01
        [HttpGet]
        public async Task<EmploymentStatus> IsEmployed(string nino, string empRef, DateTime fromDate, DateTime toDate)
        {
            _logger.LogInformation("Triggering EmploymentCheckHttpTestHarness");
            var apprenticeshipLevyService = new ApprenticeshipLevyApiClient(new HttpClient()
            {
                BaseAddress = new Uri("https://test-api.service.hmrc.gov.uk/")
            });

            var accessCode = (await _authTokenBroker.GetTokenAsync()).AccessToken;

            try
            {
                var response = await apprenticeshipLevyService.GetEmploymentStatus(accessCode, empRef,
                    nino, fromDate, toDate);

                return response;
            }
            catch (ApiHttpException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet]
        [Route("token-test")]
        public async Task<bool> TokenStubTest()
        {
            var accessCode = (await  _authTokenBroker.GetTokenAsync()).AccessToken;

            var apprenticeshipLevyService = new ApprenticeshipLevyApiClient(new HttpClient()
            {
                BaseAddress = new Uri("https://test-api.service.hmrc.gov.uk/")
            });

            var response = await apprenticeshipLevyService.GetEmploymentStatus(accessCode, "923/FEZ00059",
                "PR555555A", new DateTime(2010, 01, 01), new DateTime(2018, 01, 01));

            return response.Employed;
        }
    }
}