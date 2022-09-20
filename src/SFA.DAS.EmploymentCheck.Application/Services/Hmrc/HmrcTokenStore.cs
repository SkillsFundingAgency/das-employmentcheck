using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.EmploymentCheck.Application.Services.Hmrc
{
    //https://bryanhelms.com/2021/03/29/thread-safe-auth-token-store-using-concurrentdictionary-and-asynclazy.html
    public class HmrcTokenStore : IHmrcTokenStore
    {
        private readonly ITokenServiceApiClient _tokenService;
        private readonly IHmrcApiRetryPolicies _retryPolicies;
        private readonly ILogger<HmrcTokenStore> _logger;
        private const string TokenName = "HmrcAccessToken";

        private readonly ConcurrentDictionary<string, AsyncLazy<PrivilegedAccessToken>> _tokenCache = new ConcurrentDictionary<string, AsyncLazy<PrivilegedAccessToken>>();

        public HmrcTokenStore(ITokenServiceApiClient tokenService, IHmrcApiRetryPolicies retryPolicies, ILogger<HmrcTokenStore> logger)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _retryPolicies = retryPolicies ?? throw new ArgumentNullException(nameof(retryPolicies));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GetTokenAsync(bool forceRefresh = false)
        {
            var cacheEntry = _tokenCache.GetOrAdd(TokenName, key => new AsyncLazy<PrivilegedAccessToken>(TokenCacheFactory));

            // The cache stores Task, so we need to await to get the actual token
            var token = await GetTokenEntryFromAsyncLazy(TokenName, cacheEntry);

            if (forceRefresh || AccessTokenHasExpired(token)) {
                // If a token is expired, we should revoke it from our cache.
                // Use TryRemove because a different caller may have gotten here before us, 
                // and we don't care about throwing in this case.
                _tokenCache.TryRemove(TokenName, out _);

                cacheEntry = _tokenCache.GetOrAdd(TokenName, key => new AsyncLazy<PrivilegedAccessToken>(TokenCacheFactory));
            }

            // We have to await again, in case the token was expired after the first await
            token = await GetTokenEntryFromAsyncLazy(TokenName, cacheEntry);

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

        private bool AccessTokenHasExpired(PrivilegedAccessToken accessToken)
        {
            var expired = accessToken.ExpiryTime < DateTime.UtcNow;

            if (expired) _logger.LogInformation($"[{nameof(HmrcService)}] Access Token has expired, retrieving a new token.");

            return expired;
        }

        private async Task<PrivilegedAccessToken> TokenCacheFactory()
        {
            var policy = await _retryPolicies.GetTokenRetrievalRetryPolicy();

            return await policy.ExecuteAsync(async () =>
            {
                _logger.LogInformation($"{nameof(HmrcService)}: Refreshing access token...");

                return await _tokenService.GetPrivilegedAccessTokenAsync();
            });
        }
    }
}