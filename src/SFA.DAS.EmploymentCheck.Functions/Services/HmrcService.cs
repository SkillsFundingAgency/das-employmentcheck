using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class HmrcService : IHmrcService
    {
        private IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private ITokenServiceApiClient _tokenService;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
        }

        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, string nationalInsuranceNumber, DateTime startDate, DateTime endDate)
        {
            var token = await _tokenService.GetPrivilegedAccessTokenAsync();

            try
            {
                var response = await _apprenticeshipLevyService.GetEmploymentStatus(token.AccessCode, payeScheme, nationalInsuranceNumber, startDate, endDate);
                return response.Employed;
            }
            catch (ApiHttpException e) when (e.HttpCode == 404)
            {
                return false;
            }
        }
    }
}
