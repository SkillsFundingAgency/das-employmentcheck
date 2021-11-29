using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Configuration;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Http;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Services;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTokenServiceStubServices(this IServiceCollection services)
        {
            services.AddSingleton<ITokenServiceApiClient, TokenServiceApiClientStub>();
            services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
            services.AddSingleton<ITotpService, TotpService>();
            services.AddSingleton<IHmrcAuthTokenBroker, HmrcAuthTokenBroker>();

            services.AddSingleton<IOAuthTokenService>(s =>
            {
                var httpClient = s.GetService<IHttpClientWrapper>();
                var settings = s.GetService<IOptions<HmrcAuthTokenServiceConfiguration>>();
                return new OAuthTokenService(httpClient, settings);
            });

            return services;
        }
    }
}