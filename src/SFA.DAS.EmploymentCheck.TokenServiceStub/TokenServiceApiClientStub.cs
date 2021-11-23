using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub
{
    public class TokenServiceApiClientStub : ITokenServiceApiClient
    {
        private readonly IHmrcAuthTokenBroker _broker;

        public TokenServiceApiClientStub(IHmrcAuthTokenBroker broker)
        {
            _broker = broker;
        }

        public async Task<PrivilegedAccessToken> GetPrivilegedAccessTokenAsync()
        {
            var token = await _broker.GetTokenAsync();

            return new PrivilegedAccessToken
            {
                AccessCode = token.AccessToken,
                ExpiryTime = token.ExpiresAt
            };
        }
    }
}
