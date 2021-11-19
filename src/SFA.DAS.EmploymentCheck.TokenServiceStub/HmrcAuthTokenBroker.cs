using SFA.DAS.EmploymentCheck.TokenServiceStub.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub
{
    public class HmrcAuthTokenBroker : IHmrcAuthTokenBroker, IDisposable
    {
        private readonly IOAuthTokenService _tokenService;
        private readonly ITotpService _totpService;
        private OAuthAccessToken _cachedAccessToken;
        private CancellationTokenSource _cancellationTokenSource;

        public HmrcAuthTokenBroker(
            IOAuthTokenService tokenService,
            ITotpService totpService)
        {
            _totpService = totpService;
            _tokenService = tokenService;
        }

        public async Task<OAuthAccessToken> GetTokenAsync()
        {
            await InitialiseToken();
            return _cachedAccessToken;
        }

        private Task<OAuthAccessToken> InitialiseToken()
        {
            return GetTokenFromServiceAsync()
                .ContinueWith((task) =>
                {
                    StartTokenBackgroundRefresh();
                    return task.Result;
                });
        }

        private void StartTokenBackgroundRefresh()
        {
            DisposeCancellationToken();
        }

        private Task<OAuthAccessToken> GetTokenFromServiceAsync()
        {
            return Task.Run(async () =>
            {
                OAuthAccessToken tempToken = null;
                while (tempToken == null)
                {
                    var oneTimePassword = GetOneTimePassword();
                    tempToken = await _tokenService.GetAccessToken(oneTimePassword);

                    if (tempToken == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }

                _cachedAccessToken = tempToken;

                return _cachedAccessToken;
            });
        }

        private string GetOneTimePassword()
        {
            var privilegedToken = _totpService.Generate(_tokenService.TotpSecret);
            return privilegedToken;
        }

        public void Dispose()
        {
            DisposeCancellationToken();
        }

        public void DisposeCancellationToken()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            _cancellationTokenSource = null;
        }
    }
}
