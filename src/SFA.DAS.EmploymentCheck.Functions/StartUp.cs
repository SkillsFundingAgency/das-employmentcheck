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
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmploymentCheck.Functions.Startup))]

namespace SFA.DAS.EmploymentCheck.Functions
{
    public class Startup : FunctionsStartup
    {
        protected IConfiguration Configuration;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.UseNServiceBusContainer();

            builder.Services
                .AddNLog()
                .AddOptions()
                ;

            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();
            Configuration = configuration;

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


            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);
            logger.LogInformation($"Logger is not null: {logger != null}");
            logger.LogInformation($"Startup: using NServiceBusConnectionString={configuration["NServiceBusConnectionString"]} from config");
            logger.LogInformation($"Startup: using NServiceBusConnectionString={Environment.GetEnvironmentVariable("NServiceBusConnectionString")} from environment variables");
            
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

        public void ConfigureContainer(UpdateableServiceProvider serviceProvider)
        {
            serviceProvider.StartNServiceBus(Configuration, ServiceCollectionExtensions.ConfigurationIsLocalOrDev(Configuration["EnvironmentName"]));
        }

        private void AddNServiceBus(IFunctionsHostBuilder builder, IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);
            logger.LogInformation($"Startup AddNServiceBus: using NServiceBusConnectionString={configuration["NServiceBusConnectionString"]} from config");
            builder.Services.AddNServiceBus(logger);

            if (!configuration["NServiceBusConnectionString"].Equals("UseLearningEndpoint=true", StringComparison.CurrentCultureIgnoreCase))
            {
                Environment.SetEnvironmentVariable("NServiceBusConnectionString", configuration["NServiceBusConnectionString"]);
                builder.Services.AddNServiceBus(logger);
            }
            else
            {
                builder.Services.AddNServiceBus(
                    logger, options =>
                    {
                        options.EndpointConfiguration = endpoint =>
                        {
                            endpoint.UseTransport<LearningTransport>().StorageDirectory(
                                Path.Combine(
                                    Directory.GetCurrentDirectory()[
                                        ..Directory.GetCurrentDirectory().IndexOf("src", StringComparison.Ordinal)],
                                    @"src\.learningtransport"));
                            endpoint.UseTransport<LearningTransport>().Routing().AddRouting();

                            return endpoint;
                        };
                    });
            }
        }
    }
}