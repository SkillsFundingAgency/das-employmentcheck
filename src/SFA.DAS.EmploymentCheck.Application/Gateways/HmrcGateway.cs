using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Polly;
using Polly.Retry;
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

        private RetryPolicy GetRetryPolicy()
        {
            return Policy
                .Handle<ApiHttpException>(ex => ex.HttpCode == 429 || ex.HttpCode == 408)
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(10),
                    (exception, timespan) =>
                    {
                        _logger.Warn($"Exception calling HMRC API: ({exception.Message}). Retrying...");
                    }
                );
        }
    }
}
