using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ILogger<HmrcService> _logger;
        private ITokenServiceApiClient _tokenService;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, ILogger<HmrcService> logger)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
        }

        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, CheckApprenticeCommand checkApprenticeCommand, DateTime startDate, DateTime endDate)
        {
            var thisMethodName = $"HmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(payScheme {payeScheme}, [nationalInsuranceNumber for apprentice id {checkApprenticeCommand.Apprentice.Id}], startDate {startDate}, endDate {endDate})";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            //_logger.LogInformation($"{messagePrefix} Started.");

            var token = await _tokenService.GetPrivilegedAccessTokenAsync();

            try
            {
                var response = await _apprenticeshipLevyService.GetEmploymentStatus(
                    token.AccessCode,
                    payeScheme,
                    checkApprenticeCommand.Apprentice.NationalInsuranceNumber.Trim(),
                    startDate,
                    endDate);
                return response.Employed;
            }
            catch (ApiHttpException ex) when (ex.HttpCode == 404)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
                return false;
            }
        }
    }
}
