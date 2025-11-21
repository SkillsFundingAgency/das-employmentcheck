using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Configuration;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry;

[assembly: WebJobsStartup(typeof(SFA.DAS.EmploymentCheck.Functions.Startup))]

namespace SFA.DAS.EmploymentCheck.Functions
{
    [ExcludeFromCodeCoverage]
    public class Startup : IWebJobsStartup
    {
        private const string EnvironmentName = "EnvironmentName";

        public void Configure(IWebJobsBuilder builder)
        {
            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            var applicationDirectory = localRoot ?? azureRoot;
            builder.Services
                .AddNLog(applicationDirectory, Environment.GetEnvironmentVariable(EnvironmentName))
                .AddOptions()
                .AddMemoryCache()
                ;

            var serviceProvider = builder.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (ServiceCollectionExtensions.NotDevelopmentOrAcceptanceTests(configuration[EnvironmentName]))
            {
                configBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = configuration[EnvironmentName];
                    options.PreFixConfigurationKeys = false;
                });
            }

            configBuilder.AddJsonFile("local.settings.json", optional: true);

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddOptions();

            builder.Services.Configure<ApiRetryOptions>(config.GetSection("ApiRetryOptions"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ApiRetryOptions>>().Value);

            builder.Services.Configure<EmployerAccountApiConfiguration>(config.GetSection("AccountsInnerApi"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountApiConfiguration>>().Value);

            builder.Services.Configure<HmrcApiConfiguration>(config.GetSection("HmrcApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcApiConfiguration>>().Value);

            builder.Services.Configure<HmrcApiRateLimiterOptions>(config.GetSection("HmrcApiRateLimiterOptions"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcApiRateLimiterOptions>>().Value);

            builder.Services.Configure<TokenServiceApiClientConfiguration>(config.GetSection("TokenService"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value);

            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ApplicationSettings>>().Value);

            builder.Services.Configure<DataCollectionsApiConfiguration>(config.GetSection("DcApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<DataCollectionsApiConfiguration>>().Value);

            builder.Services.Configure<HmrcAuthTokenServiceConfiguration>(config.GetSection("HmrcAuthTokenService"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcAuthTokenServiceConfiguration>>().Value);

            builder.Services.AddTransient<IHmrcApiTelemetrySanitizer, HmrcApiTelemetrySanitizer>();
            builder.Services.AddTransient<ILearnerDataTelemetrySanitizer, LearnerDataTelemetrySanitizer>();
            builder.Services.AddSingleton<ITelemetryInitializer, TelemetryIntializer>();

            var applicationSettings = config.GetSection("ApplicationSettings").Get<ApplicationSettings>();

            builder.Services
                .AddCommandServices()
                .AddQueryServices()
                .AddApprenticeshipLevyApiClient()
                .AddEmploymentCheckService(config[EnvironmentName])
                .AddPersistenceServices()
                .AddNServiceBus(applicationSettings);
        }
    }
}