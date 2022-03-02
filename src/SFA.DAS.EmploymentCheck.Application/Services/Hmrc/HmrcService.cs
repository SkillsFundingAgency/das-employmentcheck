using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ITokenServiceApiClient _tokenService;
        private readonly ILogger<HmrcService> _logger;
        private readonly IEmploymentCheckService _employmentCheckService;
        private readonly IHmrcApiRetryPolicies _retryPolicies;
        private PrivilegedAccessToken _cachedToken;

        public HmrcService(
            ITokenServiceApiClient tokenService,
            IApprenticeshipLevyApiClient apprenticeshipLevyService,
            ILogger<HmrcService> logger,
            IEmploymentCheckService employmentCheckService,
            IHmrcApiRetryPolicies retryPolicies)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
            _employmentCheckService = employmentCheckService;
            _retryPolicies = retryPolicies;
        }

        public async Task<EmploymentCheckCacheRequest> IsNationalInsuranceNumberRelatedToPayeScheme(EmploymentCheckCacheRequest request)
        {
            EmploymentCheckCacheResponse response;

            try
            {
                await RetrieveAuthenticationToken();

                var result = await GetEmploymentStatusWithRetries(request);

                request.SetEmployed(result.Employed);

                response = EmploymentCheckCacheResponse.CreateSuccessfulCheckResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    result.Employed,
                    result.Empref);

                await _employmentCheckService.StoreCompletedCheck(request, response);
                await _retryPolicies.ReduceRetryDelay();

                return request;
            }
            catch (ApiHttpException e)
            {
                _logger.LogError($"{nameof(HmrcService)}: ApiHttpException occurred [{e}]");

                response = EmploymentCheckCacheResponse.CreateCompleteCheckErrorResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    e.ResourceUri,
                    (short)e.HttpCode);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(HmrcService)}: Exception occurred [{e}]");

                response = EmploymentCheckCacheResponse.CreateCompleteCheckErrorResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    $"{e.Message[Range.EndAt(Math.Min(8000, e.Message.Length))]}",
                    (short)HttpStatusCode.InternalServerError);
            }

            await _employmentCheckService.InsertEmploymentCheckCacheResponse(response);

            return request;
        }


        private bool AccessTokenHasExpired()
        {
            var expired = _cachedToken.ExpiryTime < DateTime.Now;
            if (expired) _logger.LogInformation($"[{nameof(HmrcService)}] Access Token has expired, retrieving a new token.");
            return expired;
        }

        private async Task<EmploymentStatus> GetEmploymentStatusWithRetries(EmploymentCheckCacheRequest request)
        {
            var policyWrap = await _retryPolicies.GetAll(() => RetrieveAuthenticationToken(true));
            var result = await policyWrap.ExecuteAsync(() => GetEmploymentStatus(request));

            return result;
        }

        private async Task<EmploymentStatus> GetEmploymentStatus(EmploymentCheckCacheRequest request)
        {
            var employmentStatus = await _apprenticeshipLevyService.GetEmploymentStatus(
                _cachedToken.AccessCode,
                request.PayeScheme,
                request.Nino,
                request.MinDate,
                request.MaxDate
            );

            return employmentStatus;
        }

        private async Task RetrieveAuthenticationToken(bool force = false)
        {
            if (force || _cachedToken == null || AccessTokenHasExpired())
            {
                var policy = await _retryPolicies.GetTokenRetrievalRetryPolicy();
                await policy.ExecuteAsync(async () =>
                {
                    _cachedToken = await _tokenService.GetPrivilegedAccessTokenAsync();
                });
            }
        }
    }
}