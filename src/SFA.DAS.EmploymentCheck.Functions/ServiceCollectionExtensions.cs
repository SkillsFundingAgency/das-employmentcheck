﻿using HMRC.ESFA.Levy.Api.Client;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Application.Services.NationalInsuranceNumber;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.EmploymentCheck.TokenServiceStub;
using SFA.DAS.Http;
using SFA.DAS.TokenService.Api.Client;
using System;
using System.IO;
using System.Net.Http;
using NLog;
using SFA.DAS.EmploymentCheck.Data;
using TokenServiceApiClientConfiguration = SFA.DAS.EmploymentCheck.Infrastructure.Configuration.TokenServiceApiClientConfiguration;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmploymentCheckService(this IServiceCollection serviceCollection, string environmentName)
        {
            serviceCollection.AddHttpClient();

            serviceCollection.AddSingleton<ApiRetryDelaySettings>();

            serviceCollection.AddSingleton<IHmrcApiOptionsRepository>(
                    s => new HmrcApiOptionsRepository(s.GetService<ApiRetryDelaySettings>(), 
                    s.GetService<IOptions<HmrcApiRateLimiterOptions>>(), 
                    s.GetService<ILogger<HmrcApiOptionsRepository>>()));

            serviceCollection.AddSingleton<IApiOptionsRepository>(s => new ApiOptionsRepository(s.GetService<IOptions<ApiRetryOptions>>()));

            serviceCollection.AddSingleton<IHmrcApiRetryPolicies, HmrcApiRetryPolicies>();
            serviceCollection.AddSingleton<IApiRetryPolicies, ApiRetryPolicies>();

            serviceCollection.AddTransient<IDcTokenService, DcTokenService>();
            serviceCollection.AddTransient<IEmploymentCheckService, EmploymentCheckService>();

            serviceCollection.AddTransient<IDataCollectionsApiClient<DataCollectionsApiConfiguration>, DataCollectionsApiClient>();

            serviceCollection.AddTransient<INationalInsuranceNumberYearsService, NationalInsuranceNumberYearsService>();
            serviceCollection.AddTransient<INationalInsuranceNumberService, NationalInsuranceNumberService>();
            serviceCollection.Decorate<INationalInsuranceNumberService, NationalInsuranceNumberServiceWithAYLookup>();
            serviceCollection.Decorate<INationalInsuranceNumberService, NationalInsuranceNumberServiceWithLogging>();            
            serviceCollection.AddTransient<ILearnerService, LearnerService>();            

            serviceCollection.AddTransient<IAzureClientCredentialHelper, AzureClientCredentialHelper>();

            serviceCollection.AddTransient<IEmployerAccountApiClient<EmployerAccountApiConfiguration>, EmployerAccountApiClient>();
            serviceCollection.AddTransient<IEmployerAccountService, EmployerAccountService>();

            serviceCollection.AddTransient<IHmrcService, HmrcService>();
            serviceCollection.AddSingleton<IHmrcTokenStore, HmrcTokenStore>();
            serviceCollection.AddSingleton<ILearnerNiNumberValidator, LearnerNiNumberValidator>();
            serviceCollection.AddSingleton<IEmployerPayeSchemesValidator, EmployerPayeSchemesValidator>();
            serviceCollection.AddSingleton<IEmploymentCheckDataValidator, EmploymentCheckDataValidator>();

            if (NotDevelopmentOrAcceptanceTests(environmentName))
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
            serviceCollection.AddTransient<IEmploymentCheckRepository, EmploymentCheckRepository>();
            serviceCollection.AddSingleton<IDataCollectionsResponseRepository, DataCollectionsResponseRepository>();
            serviceCollection.AddSingleton<IAccountsResponseRepository, AccountsResponseRepository>();
            serviceCollection.AddTransient<IEmploymentCheckCacheRequestRepository, EmploymentCheckCacheRequestRepository>();
            serviceCollection.AddTransient<IEmploymentCheckCacheResponseRepository, EmploymentCheckCacheResponseRepository>();
            serviceCollection.AddTransient<IUnitOfWork, Data.Repositories.UnitOfWork>();

            return serviceCollection;
        }

        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection, string currentDirectory, string environmentName)
        {
            if (!String.IsNullOrWhiteSpace(environmentName))
            {
                var configFileName = "nlog.config";
                if (ConfigurationIsLocalOrDev(environmentName))
                {
                    configFileName = "nlog.local.config";
                }

                LogManager.Setup()
                    .SetupExtensions(e => e.AutoLoadAssemblies(false))
                    .LoadConfigurationFromFile($"{currentDirectory}{Path.DirectorySeparatorChar}{configFileName}",
                        optional: false)
                    .LoadConfiguration(builder => builder.LogFactory.AutoShutdown = false)
                    .GetCurrentClassLogger();
            }

            serviceCollection.AddLogging((options) =>
            {
                options.AddFilter("SFA.DAS", Microsoft.Extensions.Logging.LogLevel.Information); // this is because all logging is filtered out by defualt
                options.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
                options.AddConsole();
            });

            return serviceCollection;
        }

        public static IServiceCollection AddApprenticeshipLevyApiClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IApprenticeshipLevyApiClient>(s =>
            {
                var settings = s.GetService<IOptions<HmrcApiConfiguration>>().Value;

                var httpClient = new HttpClient();

                if (!settings.BaseUrl.EndsWith("/"))
                {
                    settings.BaseUrl += "/";
                }
                httpClient.BaseAddress = new Uri(settings.BaseUrl);

                return new ApprenticeshipLevyApiClient(httpClient);
            });

            return serviceCollection;
        }

        public static bool NotDevelopmentOrAcceptanceTests(string environmentName)
        {
            return !environmentName.Equals("DEV", StringComparison.CurrentCultureIgnoreCase)
                   && !environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase)
                   && !environmentName.Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool ConfigurationIsLocalOrDev(string environmentName)
        {
            return environmentName.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase) ||
                   environmentName.Equals("DEV", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
