using System;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.EmploymentCheck.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private readonly ITokenStore _tokenStore;
        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ILogger<HmrcService> _logger;
        private readonly IEmploymentCheckService _employmentCheckService;
        private readonly IHmrcApiRetryPolicies _retryPolicies;
        private const string TokenName = "HmrcAccessToken";

        public HmrcService(
            ITokenStore tokenStore,
            IApprenticeshipLevyApiClient apprenticeshipLevyService,
            ILogger<HmrcService> logger,
            IEmploymentCheckService employmentCheckService,
            IHmrcApiRetryPolicies retryPolicies)
        {
            _tokenStore = tokenStore;
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

            await _employmentCheckService.StoreCompletedCheck(request, response);

            return request;
        }

        private async Task<EmploymentStatus> GetEmploymentStatusWithRetries(EmploymentCheckCacheRequest request)
        {
            var policyWrap = await _retryPolicies.GetAll(() => _tokenStore.GetTokenAsync(TokenName));
            var result = await policyWrap.ExecuteAsync(() => GetEmploymentStatus(request));

            return result;
        }

        private async Task<EmploymentStatus> GetEmploymentStatus(EmploymentCheckCacheRequest request)
        {
            var accessCode = await _tokenStore.GetTokenAsync(TokenName);

            var employmentStatus = await _apprenticeshipLevyService.GetEmploymentStatus(
                accessCode,
                request.PayeScheme,
                request.Nino,
                request.MinDate,
                request.MaxDate
            );

            return employmentStatus;
        }
    }

    public class CachedTokenStore : ITokenStore
    {
        private readonly ITokenServiceApiClient _tokenService;
        private readonly IHmrcApiRetryPolicies _retryPolicies;
        private readonly ILogger<CachedTokenStore> _logger;

        private readonly ConcurrentDictionary<string, AsyncLazy<PrivilegedAccessToken>> _tokenCache = new ConcurrentDictionary<string, AsyncLazy<PrivilegedAccessToken>>();

        public CachedTokenStore(ITokenServiceApiClient tokenService, IHmrcApiRetryPolicies retryPolicies, ILogger<CachedTokenStore> logger)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _retryPolicies = retryPolicies ?? throw new ArgumentNullException(nameof(retryPolicies));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool AccessTokenHasExpired(PrivilegedAccessToken accessToken)
        {
            var expired = accessToken.ExpiryTime < DateTime.UtcNow;
            if (expired) _logger.LogInformation($"[{nameof(HmrcService)}] Access Token has expired, retrieving a new token.");
            return expired;
        }

        public async Task<string> GetTokenAsync(string tokenName)
        {
            var cacheEntry = _tokenCache.GetOrAdd(tokenName, key => new AsyncLazy<PrivilegedAccessToken>(() => TokenCacheFactory(key)));

            // The cache stores Task, so we need to await to get the actual token
            var token = await GetTokenEntryFromAsyncLazy(tokenName, cacheEntry);
            if (AccessTokenHasExpired(token)) {
                // If a token is expired, we should revoke it from our cache.
                // Use TryRemove because a different caller may have gotten here before us, 
                // and we don't care about throwing in this case.
                _tokenCache.TryRemove(tokenName, out _);

                cacheEntry = _tokenCache.GetOrAdd(tokenName, key => new AsyncLazy<PrivilegedAccessToken>(() => TokenCacheFactory(key)));
            }

            // We have to await again, in case the token was expired after the first await
            token = await GetTokenEntryFromAsyncLazy(tokenName, cacheEntry);
            return token.AccessCode;
        }

        // This method makes sure that we don't keep a failed AsyncLazy in the cache if the 
        // token request fails.
        private async Task<PrivilegedAccessToken> GetTokenEntryFromAsyncLazy(string tokenName, AsyncLazy<PrivilegedAccessToken> entry)
        {
            PrivilegedAccessToken tokenEntry = null;
            try
            {
                tokenEntry = await entry;
            }
            catch
            {
                // Generating the token failed, so we should remove the key from the cache.
                _tokenCache.TryRemove(tokenName, out _);
            }

            return tokenEntry;
        }

        private async Task<PrivilegedAccessToken> TokenCacheFactory(string tokenName)
        {
            var policy = await _retryPolicies.GetTokenRetrievalRetryPolicy();
            return await policy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"{nameof(HmrcService)}: Refreshing access token...");
                return await _tokenService.GetPrivilegedAccessTokenAsync();
            });
        }
    }

    public interface ITokenStore
    {
        Task<string> GetTokenAsync(string tokenName);
    }

    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) : base(() => Task.Factory.StartNew(valueFactory)) { }

        public AsyncLazy(Func<Task<T>> taskFactory) : base(() => Task.Factory.StartNew(taskFactory).Unwrap()) { }

        // This allow awaiting the value within the AsyncLazy directly, rather than having to use .Value
        public TaskAwaiter<T> GetAwaiter() { return Value.GetAwaiter(); }
    }
}