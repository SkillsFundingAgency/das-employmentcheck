﻿using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Configuration;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Http;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Services;
using SFA.DAS.TokenService.Api.Client;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTokenServiceStubServices(this IServiceCollection services,
            HmrcAuthTokenServiceConfiguration configuration)
        {
            services.AddSingleton(configuration);
            services.AddSingleton<ITokenServiceApiClient, TokenServiceApiClientStub>();
            services.AddSingleton<IHttpClientWrapper, HttpClientWrapper>();
            services.AddSingleton<IOAuthTokenService, OAuthTokenService>();
            services.AddSingleton<ITotpService, TotpService>();
            services.AddSingleton<IHmrcAuthTokenBroker, HmrcAuthTokenBroker>();
           
            return services;
        }
    }
}