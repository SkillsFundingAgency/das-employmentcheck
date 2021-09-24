using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using SFA.DAS.Configuration.AzureTableStorage;
using System.IO;
using MediatR;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Queries.GetApprenticesToVerify;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmploymentCheck.Functions.Startup))]
namespace SFA.DAS.EmploymentCheck.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddNLog();

            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

#if DEBUG
            configBuilder.AddJsonFile("local.settings.json", optional: true);
#endif
            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddOptions();
            
            var logger = serviceProvider.GetService<ILoggerProvider>().CreateLogger(GetType().AssemblyQualifiedName);
            if (config["NServiceBusConnectionString"] == "UseDevelopmentStorage=true")
            {
                builder.Services.AddNServiceBus(logger, (options) =>
                {
                    options.EndpointConfiguration = (endpoint) =>
                    {
                        endpoint.UseTransport<LearningTransport>().StorageDirectory(Path.Combine(Directory.GetCurrentDirectory().Substring(0, Directory.GetCurrentDirectory().IndexOf("src")), @"src\SFA.DAS.EmployerIncentives.Functions.TestConsole\.learningtransport"));
                        return endpoint;
                    };
                });
            }
            else
            {
                builder.Services.AddNServiceBus(logger);
            }

            builder.Services.AddMediatR(typeof(GetApprenticesToVerifyRequest).Assembly);

            builder.Services.Configure<AccountsApiConfiguration>(configuration.GetSection("AccountsInnerApi"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<AccountsApiConfiguration>>().Value);

            builder.Services.AddEmploymentCheckService();
        }
    }
}
