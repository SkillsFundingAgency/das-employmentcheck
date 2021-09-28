using System;
using HMRC.ESFA.Levy.Api.Client;
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
using SFA.DAS.Http;
using SFA.DAS.TokenService.Api.Client;
using TokenServiceApiClientConfiguration = SFA.DAS.EmploymentCheck.Functions.Configuration.TokenServiceApiClientConfiguration;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmploymentCheckService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHttpClient();
            serviceCollection.AddTransient<IAccountsApiClient, AccountsApiClient>();
            serviceCollection.AddTransient<IAccountsService, AccountsService>();
            serviceCollection.AddTransient<IHmrcService, HmrcService>();
            serviceCollection.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
            serviceCollection.AddTransient<IEmploymentChecksRepository, EmploymentChecksRepository>();
            serviceCollection.AddHmrcClient();
            serviceCollection.AddTransient<ITokenServiceApiClientConfiguration, TokenServiceApiClientConfiguration>();
            serviceCollection.AddTransient<ITokenServiceApiClient, TokenServiceApiClient>();
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
                    .WithDefaultHeaders()
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
