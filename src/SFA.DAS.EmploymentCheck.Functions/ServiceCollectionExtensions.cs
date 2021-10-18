using System;
using HMRC.ESFA.Levy.Api.Client;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Clients;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Services;
using SFA.DAS.EmploymentCheck.Functions.Services.Fakes;
using SFA.DAS.Http;
using SFA.DAS.TokenService.Api.Client;
using TokenServiceApiClientConfiguration = SFA.DAS.EmploymentCheck.Functions.Configuration.TokenServiceApiClientConfiguration;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmploymentCheckService(this IServiceCollection serviceCollection, string environmentName)
        {
            serviceCollection.AddHttpClient();
            serviceCollection.AddTransient<IAccountsApiClient, AccountsApiClient>();

#if DEBUG
            // For local development use the Stubs
            serviceCollection.AddTransient<IAccountsService, AccountsServiceStub>();
            serviceCollection.AddSingleton<IRandomNumberService, RandomNumberService>(); // used by the HrmcServiceStub
            serviceCollection.AddTransient<IHmrcService, HmrcServiceStub>();

#else
            serviceCollection.AddTransient<IAccountsService, AccountsService>();
            serviceCollection.AddTransient<IHmrcService, HmrcService>();
#endif


            serviceCollection.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
            if (!environmentName.Equals("DEV", StringComparison.CurrentCultureIgnoreCase) && !environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                serviceCollection.AddSingleton(new AzureServiceTokenProvider());
            }

#if DEBUG
            // For local development use the EmploymentChecksRepositoryStub
            serviceCollection.AddTransient<IEmploymentChecksRepository, EmploymentChecksRepositoryStub>();
#else
            serviceCollection.AddTransient<IEmploymentChecksRepository, EmploymentChecksRepository>();
#endif

            serviceCollection.AddHmrcClient();
            serviceCollection.AddTransient<ITokenServiceApiClient, TokenServiceApiClient>(s =>
            {
                var config = s.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value;
                return new TokenServiceApiClient(config);
            });
            return serviceCollection;
        }

        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection)
        {
            var nLogConfiguration = new NLogConfiguration();

            serviceCollection.AddLogging((options) =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Information); // this is because all logging is filtered out by defualt
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();

                nLogConfiguration.ConfigureNLog();
            });

            return serviceCollection;
        }

        private static IServiceCollection AddHmrcClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IApprenticeshipLevyApiClient>(s =>
            {
                var settings = s.GetService<IOptions<HmrcApiSettings>>().Value;

                var clientBuilder = new HttpClientBuilder()
                    .WithLogging(s.GetService<ILoggerFactory>());

                var httpClient = clientBuilder.Build();

                if (!settings.BaseUrl.EndsWith("/"))
                {
                    settings.BaseUrl += "/";
                }
                httpClient.BaseAddress = new Uri(settings.BaseUrl);

                return new ApprenticeshipLevyApiClient(httpClient);
            });

            return serviceCollection;
        }
    }
}
