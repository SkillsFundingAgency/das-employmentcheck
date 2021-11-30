using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
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

        public async Task<ApprenticeEmploymentCheckMessageModel> CheckApprenticeEmploymentStatus_Client(
            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            var thisMethodName = $"{nameof(HmrcClient)}.CheckApprenticeEmploymentStatus_Client()";

            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModelResult = null;
            try
            {
                if (apprenticeEmploymentCheckMessageModel != null)
                {
                    apprenticeEmploymentCheckMessageModelResult = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(apprenticeEmploymentCheckMessageModel);

                    if (apprenticeEmploymentCheckMessageModelResult == null)
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

            return apprenticeEmploymentCheckMessageModelResult;
        }
    }
}
