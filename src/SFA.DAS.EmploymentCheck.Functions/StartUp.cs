﻿using MediatR;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks;
using System.IO;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.TokenServiceStub.Configuration;

[assembly: FunctionsStartup(typeof(SFA.DAS.EmploymentCheck.Functions.Startup))]

namespace SFA.DAS.EmploymentCheck.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services
                .AddNLog()
                .AddOptions()
                .AddApprenticeshipLevyApiClient()
                .AddHashingService();

            var serviceProvider = builder.Services.BuildServiceProvider();

            var configuration = serviceProvider.GetService<IConfiguration>();

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            configBuilder.AddJsonFile("local.settings.json", optional: true);

            var config = configBuilder.Build();
            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.Services.AddOptions();

            // MediatR configuration
            //builder.Services.AddMediatR(typeof(GetApprenticesToVerifyRequest).Assembly);
            builder.Services.AddMediatR(typeof(GetApprenticeEmploymentChecksQueryRequest).Assembly);

            // SubmitLearnerData API Configuration
            builder.Services.Configure<SubmitLearnerDataApiConfiguration>(
                config.GetSection("SubmitLearnersDataApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<SubmitLearnerDataApiConfiguration>>().Value);

            // Accounts API Configuration
            builder.Services.Configure<EmployerAccountApiConfiguration>(config.GetSection("AccountsInnerApi"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<EmployerAccountApiConfiguration>>().Value);

            // HMRC API Settings
            builder.Services.Configure<HmrcApiConfiguration>(config.GetSection("HmrcApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcApiConfiguration>>().Value);

            // Token Service API Configuration
            builder.Services.Configure<TokenServiceApiClientConfiguration>(config.GetSection("TokenService"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<TokenServiceApiClientConfiguration>>().Value);

            // Application Settings
            builder.Services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<ApplicationSettings>>().Value);

            //DC Api Settings
            builder.Services.Configure<DcApiSettings>(config.GetSection("DcApiSettings"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<DcApiSettings>>().Value);

            // HmrcAuthTokenService Settings
            builder.Services.Configure<HmrcAuthTokenServiceConfiguration>(config.GetSection("HmrcAuthTokenService"));
            builder.Services.AddSingleton(cfg => cfg.GetService<IOptions<HmrcAuthTokenServiceConfiguration>>().Value);

            builder.Services.AddEmploymentCheckService(config["EnvironmentName"]);
        }
    }
}
