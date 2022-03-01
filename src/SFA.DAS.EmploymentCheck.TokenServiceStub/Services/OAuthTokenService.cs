using SFA.DAS.EmploymentCheck.TokenServiceStub.Configuration;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub.Services
{
    public class OAuthTokenService : IOAuthTokenService
    {
        private readonly IHttpClientWrapper _httpClient;
        private readonly HmrcAuthTokenServiceConfiguration _configuration;

        public OAuthTokenService(IHttpClientWrapper httpClient, IOptions<HmrcAuthTokenServiceConfiguration> configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration.Value;
        }

        public async Task<OAuthAccessToken> GetAccessToken(string clientSecret)
        {
            var request = new OAuthTokenRequest
            {
                ClientId = _configuration.ClientId,
                ClientSecret = $"{clientSecret}{_configuration.ClientSecret}",
                GrantType = "client_credentials",
                Scopes = "read:apprenticeship-levy"
            };

            var hmrcToken = await _httpClient.Post<OAuthTokenResponse>(_configuration.TokenUrl, request);

            return new OAuthAccessToken
            {
                AccessToken = hmrcToken.AccessToken,
                RefreshToken = hmrcToken.RefreshToken,
                ExpiresAt = DateTime.Now.AddSeconds(hmrcToken.ExpiresIn),
                Scope = hmrcToken.Scope,
                TokenType = hmrcToken.TokenType
            };
        }

        public string TotpSecret => _configuration.TotpSecret;
    }
}
