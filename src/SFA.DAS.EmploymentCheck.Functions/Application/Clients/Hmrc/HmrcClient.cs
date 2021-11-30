using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc
{
    public class HmrcClient
        : IHmrcClient
    {
        private const string ThisClassName = "\n\nHmrcClient";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private IHmrcService _hmrcService;
        private ILogger<IHmrcClient> _logger;

        /// <summary>
        /// The Client class that calls the Hmrc services
        /// </summary>
        /// <param name="employmentCheckDbConfiguration"></param>
        /// <param name="employmentCheckService"></param>
        /// <param name="logger"></param>
        public HmrcClient(
            IHmrcService  hmrcService,
            ILogger<HmrcClient> logger)
        {
            _hmrcService = hmrcService;
            _logger = logger;
        }

        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database
        /// </summary>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public async Task<EmploymentCheckMessage> CheckApprenticeEmploymentStatus_Client(
            EmploymentCheckMessage apprenticeEmploymentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.CheckApprenticeEmploymentStatus_Client()";

            EmploymentCheckMessage employmentCheckMessageResult = null;
            try
            {
                if (apprenticeEmploymentCheckMessageModel != null)
                {
                    employmentCheckMessageResult = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(apprenticeEmploymentCheckMessageModel);

                    if (employmentCheckMessageResult == null)
                    {
                        _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModelResult value returned from the IsNationalInsuranceNumberRelatedToPayeScheme() call returned null.");
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModelResult input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckMessageResult;
        }
    }
}
