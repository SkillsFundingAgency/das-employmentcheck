using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
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
using SFA.DAS.UnitOfWork.NServiceBus.Features.ClientOutbox.DependencyResolution.Microsoft;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry;
using Newtonsoft.Json;
using SFA.DAS.Encoding;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights;
using SFA.DAS.EmploymentCheck.Application.Telemetry;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmploymentCheck.Functions.Startup))]
namespace SFA.DAS.EmploymentCheck.Functions
{
    [ExcludeFromCodeCoverage]
    public class Startup : FunctionsStartup
    {
        private const string EncodingConfigKey = "SFA.DAS.Encoding";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var localRoot = Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot");
            var azureRoot = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
            var applicationDirectory = localRoot ?? azureRoot;
        
            builder.Services
                .AddNLog(applicationDirectory, Environment.GetEnvironmentVariable("EnvironmentName"))
                .AddOptions()
                .AddMemoryCache()
                ;

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
                    options.ConfigurationKeysRawJsonResult = new[] { EncodingConfigKey };
                });
            }

            var appInsightsInstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
            var telemetryConfiguration = new TelemetryConfiguration(appInsightsInstrumentationKey);
            builder.Services.AddTransient(sp => new TelemetryConfiguration(appInsightsInstrumentationKey));
            builder.Services.AddSingleton<TelemetryClient>();
            builder.Services.AddTransient<ITelemetryClient, TelemetryWrapper>();

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

            //HRMC Api Telemetry Processor
            builder.Services.AddSingleton<ITelemetryInitializer, TelemetryIntializer>();

            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);
            var applicationSettings = config.GetSection("ApplicationSettings").Get<ApplicationSettings>();

            if (!config["EnvironmentName"]
                    .Equals("LOCAL_ACCEPTANCE_TESTS", StringComparison.CurrentCultureIgnoreCase))
            {
                var encodingConfigJson = config.GetSection(EncodingConfigKey).Value;
                var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
                builder.Services.AddSingleton(encodingConfig);
            }
            else
            {
                var encodingConfigJson = File.ReadAllText(Directory.GetCurrentDirectory() + "\\local.encoding.json");
                var encodingConfig = JsonConvert.DeserializeObject<EncodingConfig>(encodingConfigJson);
                builder.Services.AddSingleton(encodingConfig);
            }
            builder.Services.AddSingleton<IEncodingService, EncodingService>();

            
        }
    }
}