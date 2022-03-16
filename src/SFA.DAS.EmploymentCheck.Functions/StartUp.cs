using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Configuration;
using System.IO;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmploymentCheck.Functions.Startup))]

namespace SFA.DAS.EmploymentCheck.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddNLog()
                .AddOptions();

            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (ServiceCollectionExtensions.NotDevelopmentOrAcceptanceTests(configuration["EnvironmentName"]))
            {
                configBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = false;
                });
            }

            configBuilder.AddJsonFile("local.settings.json", optional: true);

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddOptions();

            // Accounts API Configuration
            builder.Services.Configure<EmployerAccountApiConfiguration>(config.GetSection("AccountsInnerApi"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountApiConfiguration>>().Value);

            // HMRC API Configuration
            builder.Services.Configure<HmrcApiConfiguration>(config.GetSection("HmrcApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcApiConfiguration>>().Value);

            // Token Service API Configuration
            builder.Services.Configure<TokenServiceApiClientConfiguration>(config.GetSection("TokenService"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value);

            // Application Configuration
            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ApplicationSettings>>().Value);

            //DC Api Settings
            builder.Services.Configure<DataCollectionsApiConfiguration>(config.GetSection("DcApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<DataCollectionsApiConfiguration>>().Value);

            // HmrcAuthTokenService Settings
            builder.Services.Configure<HmrcAuthTokenServiceConfiguration>(config.GetSection("HmrcAuthTokenService"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcAuthTokenServiceConfiguration>>().Value);

            builder.Services
                .AddCommandServices()
                .AddQueryServices()
                .AddApprenticeshipLevyApiClient()
                .AddHashingService()
                .AddEmploymentCheckService(config["EnvironmentName"])
                .AddPersistenceServices()
                .AddNServiceBusClientUnitOfWork()
            ;

            AddNServiceBus(builder, serviceProvider, configuration);
        }

        private void AddNServiceBus(IFunctionsHostBuilder builder, IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);

            if (ServiceCollectionExtensions.NotDevelopmentOrAcceptanceTests(configuration["EnvironmentName"]))
            {
                builder.Services.AddNServiceBus(logger);
            }
            else if (ServiceCollectionExtensions.ConfigurationIsLocalOrDev(configuration["EnvironmentName"]))
            {
                builder.Services.AddNServiceBus(
                    logger,
                    (options) =>
                    {
                        if (configuration["NServiceBusConnectionString"] == "UseLearningEndpoint=true")
                        {
                            options.EndpointConfiguration = (endpoint) =>
                            {
                                var dir = configuration.GetValue<string>("UseLearningEndpointStorageDirectory");
                                var altDir = Path.Combine(
                                    Directory.GetCurrentDirectory()[
                                        ..Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)],
                                    @"src\SFA.DAS.EmploymentCheck.Functions\.learningtransport");

                                endpoint.UseTransport<LearningTransport>().StorageDirectory(configuration.GetValue(
                                    "ApplicationSettings:UseLearningEndpointStorageDirectory", altDir
                                ));
                                endpoint.UseTransport<LearningTransport>().Routing().AddRouting();
                                return endpoint;
                            };
                        }
                    });
            }
        }
    }
}