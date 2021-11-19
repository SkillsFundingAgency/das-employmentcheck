using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Configuration;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

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

        public static void Initialise(IServiceCollection serviceCollection,
            HmrcAuthTokenServiceConfiguration configuration)
        {

        }
    }
}
