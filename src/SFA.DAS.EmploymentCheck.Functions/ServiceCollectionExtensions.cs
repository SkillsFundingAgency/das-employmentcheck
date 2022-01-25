﻿using HMRC.ESFA.Levy.Api.Client;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.Http;
using SFA.DAS.TokenService.Api.Client;
using System;
using SFA.DAS.EmploymentCheck.TokenServiceStub;
using SFA.DAS.HashingService;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using TokenServiceApiClientConfiguration = SFA.DAS.TokenService.Api.Client.TokenServiceApiClientConfiguration;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmploymentCheckService(this IServiceCollection serviceCollection, string environmentName)
        {
            serviceCollection.AddHttpClient();
            serviceCollection.AddTransient<IEmploymentCheckClient, EmploymentCheckClient>();
            serviceCollection.AddTransient<ILearnerClient, LearnerClient>();
            serviceCollection.AddTransient<IEmployerAccountClient, EmployerAccountClient>();
            serviceCollection.AddTransient<IHmrcClient, HmrcClient>();

            serviceCollection.AddSingleton<IHmrcApiOptionsRepository>(s =>
            {
                var hmrcApiRateLimiterConfiguration = new HmrcApiRateLimiterConfiguration
                {
                    EnvironmentName = Environment.GetEnvironmentVariable("EnvironmentName"),
                    StorageAccountConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
                };
                return new HmrcApiOptionsRepository(hmrcApiRateLimiterConfiguration, s.GetService<ILogger<HmrcApiOptionsRepository>>());
            });

            serviceCollection.AddTransient<IDcTokenService, DcTokenService>();
            serviceCollection.AddTransient<IEmploymentCheckService, EmploymentCheckService>();
            serviceCollection.AddTransient<ILearnerService, LearnerService>();
            serviceCollection.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();
            serviceCollection.AddTransient<IEmployerAccountService, EmployerAccountService>();
            serviceCollection.AddSingleton<IHmrcService, HmrcService>();

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

        public static IServiceCollection AddPersistenceServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IEmploymentCheckRepository, EmploymentCheckRepository>();
            serviceCollection.AddSingleton<IDataCollectionsResponseRepository, DataCollectionsResponseRepository>();
            serviceCollection.AddSingleton<IAccountsResponseRepository, AccountsResponseRepository>();
            serviceCollection.AddSingleton<IEmploymentCheckCacheRequestRepository, EmploymentCheckCacheRequestRepository>();
            serviceCollection.AddSingleton<IEmploymentCheckCacheResponseRepository, EmploymentCheckCacheResponseRepository>();

            return serviceCollection;
        }

        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection)
        {
            var nLogConfiguration = new NLogConfiguration();

            serviceCollection.AddLogging(options =>
            {
                options.AddFilter("SFA.DAS", LogLevel.Information);
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
