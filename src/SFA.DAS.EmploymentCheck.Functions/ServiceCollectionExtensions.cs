using HMRC.ESFA.Levy.Api.Client;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.StubsSubmitLearnerData;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using SFA.DAS.Http;
using SFA.DAS.TokenService.Api.Client;
using System;
using SFA.DAS.EmploymentCheck.TokenServiceStub;
using SFA.DAS.HashingService;
using TokenServiceApiClientConfiguration = SFA.DAS.EmploymentCheck.Functions.Configuration.TokenServiceApiClientConfiguration;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmploymentCheckService(this IServiceCollection serviceCollection, string environmentName)
        {
            serviceCollection.AddHttpClient();
            serviceCollection.AddTransient<IEmploymentCheckClient, EmploymentCheckClient>();
            serviceCollection.AddTransient<IEmployerAccountClient, EmployerAccountClient>();
            serviceCollection.AddTransient<ISubmitLearnerDataClient, SubmitLearnerDataClient>();
            serviceCollection.AddTransient<IEmployerAccountApiClient, EmployerAccountApiClient>();
            serviceCollection.AddTransient<IHmrcClient, HmrcClient>();

            // #if DEBUG
            // For local development use the Stubs

           // //serviceCollection.AddTransient<IEmploymentCheckService, EmploymentCheckServiceStub>();
           // serviceCollection.AddTransient<IEmploymentCheckService, EmploymentCheckServiceStub>();
           // serviceCollection.AddTransient<ISubmitLearnerDataService, SubmitLearnerDataServiceStub>();
           //// serviceCollection.AddTransient<IEmployerAccountService, EmployerAccountServiceStub>();
            //serviceCollection.AddTransient<IHmrcService, HmrcServiceStub>();

            serviceCollection.AddSingleton<IHmrcApiOptionsRepository>(s =>
            {
                var hmrcApiRateLimiterConfiguration = new HmrcApiRateLimiterConfiguration
                {
                    EnvironmentName = Environment.GetEnvironmentVariable("EnvironmentName"),
                    StorageAccountConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
                };
                return new HmrcApiOptionsRepository(hmrcApiRateLimiterConfiguration);
            });

            serviceCollection.AddSingleton<IHmrcService, HmrcService>();
            //serviceCollection.AddTransient<IEmploymentCheckService, EmploymentCheckService>();

            //serviceCollection.AddTransient<ISubmitLearnerDataService, SubmitLearnerDataService>();
            serviceCollection.AddTransient<ISubmitLearnerDataService, SubmitLearnerDataServiceStub>();
            //serviceCollection.AddTransient<IEmployerAccountService, EmployerAccountService>();
            serviceCollection.AddTransient<IEmployerAccountService, EmployerAccountServiceStub>();
            //serviceCollection.AddTransient<IHmrcService, HmrcService>();
            serviceCollection.AddTransient<IHmrcService, HmrcServiceStub>();

            //serviceCollection.AddTransient<IDcTokenService, DcTokenService>();
            //erviceCollection.AddTransient<IEmploymentCheckService, EmploymentCheckService>();
            serviceCollection.AddTransient<IEmploymentCheckService, EmploymentCheckServiceStub>();

            //serviceCollection.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
            if (!environmentName.Equals("DEV", StringComparison.CurrentCultureIgnoreCase) && !environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                serviceCollection.AddSingleton(new AzureServiceTokenProvider());
            }

            if (environmentName == "PROD")
            {
                serviceCollection.AddTransient<ITokenServiceApiClient, TokenServiceApiClient>(s =>
                {
                    var config = s.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value;
                    return new TokenServiceApiClient(config);
                });
            }
            else
            {
                serviceCollection.AddTokenServiceStubServices();
            }

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

        public static IServiceCollection AddApprenticeshipLevyApiClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IApprenticeshipLevyApiClient>(s =>
            {
                var settings = s.GetService<IOptions<HmrcApiConfiguration>>().Value;

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


        public static IServiceCollection AddHashingService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IHashingService>(c =>
            {
                var settings = c.GetService<IOptions<ApplicationSettings>>().Value;
                return new HashingService.HashingService(settings.AllowedHashstringCharacters, settings.Hashstring);
            });

            return serviceCollection;
        }
    }
}
