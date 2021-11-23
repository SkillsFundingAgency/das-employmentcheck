using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.TokenService.Api.Client;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ILogger<HmrcService> _logger;
        private readonly ITokenServiceApiClient _tokenService;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, ILogger<HmrcService> logger)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
        }

        public async Task<ApprenticeEmploymentCheckMessageModel> IsNationalInsuranceNumberRelatedToPayeScheme(
            ApprenticeEmploymentCheckMessageModel request)
        {
            var token = await _tokenService.GetPrivilegedAccessTokenAsync();

            try
            {
                var result = await _apprenticeshipLevyService.GetEmploymentStatus(
                    token.AccessCode,
                    request.PayeScheme,
                    request.NationalInsuranceNumber,
                    request.StartDateTime,
                    request.EndDateTime
                );

                request.IsEmployed = result.Employed;
                request.EmploymentCheckedDateTime = DateTime.UtcNow;
            }
            catch (ApiHttpException e) when (e.HttpCode == 404)
            {
                request.IsEmployed = false;
                request.EmploymentCheckedDateTime = DateTime.UtcNow;
            }

            return request;
        }
    }
}
