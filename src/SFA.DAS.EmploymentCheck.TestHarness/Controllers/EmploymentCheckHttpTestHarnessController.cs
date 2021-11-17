using HMRC.ESFA.Levy.Api.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TestHarness.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmploymentCheckHttpTestHarnessController : ControllerBase
    {
        private readonly ILogger<EmploymentCheckHttpTestHarnessController> _logger;

        public EmploymentCheckHttpTestHarnessController(ILogger<EmploymentCheckHttpTestHarnessController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// EmploymentCheckHttpTestHarness for single employee check
        /// https://skillsfundingagency.atlassian.net/browse/HTB-17
        /// </summary>
        /// <param name="accessCode">authentication access code token</param>
        /// <param name="nino">national insurance number</param>
        /// <param name="empRef">employer reference</param>
        /// <param name="fromDate">from date</param>
        /// <param name="toDate">to date</param>
        /// <returns>Employed true/false</returns>
        /// sample request: https://localhost:5001/EmploymentCheckHttpTestHarness?accessCode=*******************&nino=PR555555A&empRef=923%2FEZ00059&fromDate=2010-01-01&toDate=2018-01-01

        [HttpGet]
        public async Task<bool> IsEmployed(string accessCode, string nino, string empRef, DateTime fromDate, DateTime toDate)
        {
            _logger.LogInformation("Triggering EmploymentCheckHttpTestHarness");
            var apprenticeshipLevyService = new ApprenticeshipLevyApiClient(new HttpClient()
            {
                BaseAddress = new Uri("https://test-api.service.hmrc.gov.uk/")
            });

            var response = await apprenticeshipLevyService.GetEmploymentStatus(accessCode, empRef,
                nino, fromDate, toDate);

            return response.Employed;
        }
    }
}