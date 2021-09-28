using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Polly;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class HmrcService : IHmrcService
    {
        private AsyncPolicy _retryPolicy;
        private IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private ITokenServiceApiClient _tokenService;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, HmrcApiSettings hmrcApiSettings)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;

            _retryPolicy = GetRetryPolicy(hmrcApiSettings.RetryDelay);
        }

        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, string nationalInsuranceNumber, DateTime startDate, DateTime endDate)
        {
            return await _retryPolicy.ExecuteAsync<bool>(async () =>
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
            });
        }

        private AsyncPolicy GetRetryPolicy(int retryDelay)
        {
            var retryPolicy = Policy.Handle<ApiHttpException>(ex => ex.HttpCode == 429 || ex.HttpCode == 408)
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromMilliseconds(retryDelay));

            return retryPolicy.WrapAsync(
                Policy.Handle<ApiHttpException>(ex => ex.HttpCode == 503 || ex.HttpCode == 500)
                    .WaitAndRetryAsync(10, i => TimeSpan.FromMilliseconds(retryDelay)));
        }
    }
}
