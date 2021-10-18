using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Commands.CheckApprentice;
using SFA.DAS.EmploymentCheck.Functions.Services.Fakes;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Functions.Services
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

            _logger.LogInformation($"{messagePrefix} Started.");

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
            catch (ApiHttpException e) when (e.HttpCode == 404)
            {
                return false;
            }
        }
    }
}
