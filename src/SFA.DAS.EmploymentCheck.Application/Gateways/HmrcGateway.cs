using System;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Azure;
using Polly;
using SFA.DAS.NLog.Logger;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.Application.Gateways
{
    public class HmrcGateway : IHmrcGateway
    {
        private ITokenServiceApiClient _tokenService;
        private IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ILog _logger;
        private readonly Policy _retryPolicy;

        public HmrcGateway(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, ILog logger)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;

            _retryPolicy = GetRetryPolicy();
        }

        public async Task<bool> IsNationalInsuranceNumberRelatedToPayeScheme(string payeScheme, string nationalInsuranceNumber, DateTime startDate)
        {
            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var token = await _tokenService.GetPrivilegedAccessTokenAsync();

                var response = await _apprenticeshipLevyService.GetEmploymentStatus(token.AccessCode, payeScheme, nationalInsuranceNumber, startDate, DateTime.Now.Date);

                return response.Employed;
            });
        }

        private Policy GetRetryPolicy()
        {
            var retryDelay = int.Parse(CloudConfigurationManager.GetSetting("HmrcRetryDelay"));
            var retryPolicy = Policy.Handle<ApiHttpException>(ex => ex.HttpCode == 429 || ex.HttpCode == 408)
                    .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromMilliseconds(retryDelay), (exception, timespan) => LogWarning(exception));

            return retryPolicy.WrapAsync(
                Policy.Handle<ApiHttpException>(ex => ex.HttpCode == 503 || ex.HttpCode == 500)
                    .WaitAndRetryAsync(5, i => TimeSpan.FromMilliseconds(retryDelay), (exception, timespan) => LogWarning(exception)));
        }

        private void LogWarning(Exception exception)
        {
            _logger.Warn($"Exception calling HMRC API: ({exception.Message}). Retrying...");
        }
    }
}
