﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;
using System.IO;
using MediatR;
using Microsoft.Extensions.Options;
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
            builder.Services.AddOptions();

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

            builder.Services.AddMediatR(typeof(GetApprenticesToVerifyRequest).Assembly);

            builder.Services.Configure<AccountsApiConfiguration>(config.GetSection("AccountsInnerApi"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<AccountsApiConfiguration>>().Value);
            builder.Services.Configure<HmrcApiSettings>(config.GetSection("HmrcApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcApiSettings>>().Value);
            builder.Services.Configure<TokenServiceApiClientConfiguration>(config.GetSection("TokenService"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value);
            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ApplicationSettings>>().Value);

            builder.Services.AddEmploymentCheckService(config["EnvironmentName"]);
        }
    }
}
