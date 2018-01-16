using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Application.Gateways
{
    public class HmrcGateway : IHmrcGateway
    {
        private ITokenServiceApiClient _tokenService;
        private IApprenticeshipLevyApiClient _apprenticeshipLevyService;

        public HmrcGateway(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
        }

        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, string nationalInsuranceNumber, DateTime startDate)
        {
            var token = await _tokenService.GetPrivilegedAccessTokenAsync();

            var response = await _apprenticeshipLevyService.GetEmploymentStatus(token.AccessCode, payeScheme, nationalInsuranceNumber, startDate);

            return response.Employed;
        }
    }
}
