using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc
{
    public class HmrcClient : IHmrcClient
    {
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        private readonly IHmrcService _hmrcService;
        private readonly ILogger<IHmrcClient> _logger;

        public HmrcClient(IHmrcService  hmrcService, ILogger<HmrcClient> logger)
        {
            _hmrcService = hmrcService;
            _logger = logger;
        }

        /// <summary>
        /// Sends the employmentCheckMessage to the HMRC API
        /// </summary>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public async Task<EmploymentCheckMessage> CheckEmploymentStatus_Client(
            EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"{nameof(HmrcClient)}.CheckEmploymentStatus_Client()";

            EmploymentCheckMessage employmentCheckMessageResult = null;
            try
            {
                if (employmentCheckMessage != null &&
                    employmentCheckMessage.Id > 0)
                {
                    employmentCheckMessageResult = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(employmentCheckMessage);

                    if (employmentCheckMessageResult == null)
                    {
                        _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessageResult value returned from the IsNationalInsuranceNumberRelatedToPayeScheme() call returned null.");
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessageResult input parameter is null.");
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
