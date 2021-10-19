﻿using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SFA.DAS.Configuration.AzureTableStorage;
using System.IO;
using MediatR;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesToVerify;

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
            //configBuilder.AddAzureTableStorage(options =>
            //{
            //    options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
            //    options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
            //    options.EnvironmentName = configuration["EnvironmentName"];
            //    options.PreFixConfigurationKeys = false;
            //});

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddOptions();

            // Mediatr configuration
            builder.Services.AddMediatR(typeof(GetApprenticesToVerifyRequest).Assembly);

            // TODO: Learners API Configuration
            builder.Services.Configure<LearnersApiConfiguration>(config.GetSection("LearnersApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<LearnersApiConfiguration>>().Value);

            // Accounts API Configuration
            builder.Services.Configure<AccountsApiConfiguration>(config.GetSection("AccountsInnerApi"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<AccountsApiConfiguration>>().Value);

            // HMRC API Settings
            builder.Services.Configure<HmrcApiSettings>(config.GetSection("HmrcApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcApiSettings>>().Value);

            // Token Service API Configuration
            builder.Services.Configure<TokenServiceApiClientConfiguration>(config.GetSection("TokenService"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value);

            // Application Settings
            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ApplicationSettings>>().Value);

            builder.Services.AddEmploymentCheckService(config["EnvironmentName"]);
        }
    }
}
