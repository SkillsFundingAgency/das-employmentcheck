using SFA.DAS.EmploymentCheck.TokenServiceStub.Services;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub
{
    public class HmrcAuthTokenBroker : IHmrcAuthTokenBroker
    {
        private readonly IOAuthTokenService _tokenService;
        private readonly ITotpService _totpService;

        public HmrcAuthTokenBroker(
            IOAuthTokenService tokenService,
            ITotpService totpService)
        {
            _totpService = totpService;
            _tokenService = tokenService;
        }

        public Task<OAuthAccessToken> GetTokenAsync()
        {
            return Task.Run(async () =>
            {
                OAuthAccessToken token = null;
                while (token == null)
                {
                    var oneTimePassword = GetOneTimePassword();
                    token = await _tokenService.GetAccessToken(oneTimePassword);

                    if (token == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }

                return token;
            });
        }

        private string GetOneTimePassword()
        {
            var privilegedToken = _totpService.Generate(_tokenService.TotpSecret);
           
            return privilegedToken;
        }
    }
}
