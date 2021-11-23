using SFA.DAS.EmploymentCheck.TokenServiceStub.Configuration;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Http;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub.Services
{
    public class OAuthTokenService : IOAuthTokenService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly HmrcAuthTokenServiceConfiguration _configuration;

        public OAuthTokenService(IHttpClientWrapper httpClient, HmrcAuthTokenServiceConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<OAuthAccessToken> GetAccessToken(string oneTimePassword)
        {
            var request = new OAuthTokenRequest
            {
                ClientId = _configuration.TokenClientId,
                ClientSecret = $"{oneTimePassword}{_configuration.TokenSecret}",
                GrantType = "client_credentials",
                Scopes = "read:apprenticeship-levy"
            };
            var hmrcToken = await _httpClient.Post<OAuthTokenResponse>(_configuration.TokenUri, request);

            return new OAuthAccessToken
            {
                AccessToken = hmrcToken.AccessToken,
                RefreshToken = hmrcToken.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddSeconds(hmrcToken.ExpiresIn),
                Scope = hmrcToken.Scope,
                TokenType = hmrcToken.TokenType
            };
        }

        public string TotpSecret => _configuration.TotpSecret;
    }
}
