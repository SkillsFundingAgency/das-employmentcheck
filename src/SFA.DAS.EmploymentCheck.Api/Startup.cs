using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.Api.Configuration;
using SFA.DAS.EmploymentCheck.Api.Extensions;
using System;
using System.IO;

namespace SFA.DAS.EmploymentCheck.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(Configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            configBuilder.AddJsonFile("appsettings.Development.json", optional: true);

            configBuilder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = Configuration["ConfigNames"].Split(",");
                options.StorageConnectionString = Configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = Configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            Configuration = configBuilder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks();
            services.AddNLogForApi();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SFA.DAS.EmploymentCheck.Api", Version = "v1.0"});
            });

       
            services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));

            services.Configure<EmploymentCheckSettings>(Configuration.GetSection("ApplicationSettings"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EmploymentCheckSettings>>().Value);

            if (!Configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase) && !Configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddSingleton(new AzureServiceTokenProvider());

                var azureAdConfiguration = Configuration
                    .GetSection("AzureAd")
                    .Get<AzureActiveDirectoryConfiguration>();

                services.AddAuthentication(azureAdConfiguration);
            }

            services
                .AddRepositories()
                .AddServices()
                .AddHandlers()
                ;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SFA.DAS.EmploymentCheck.Api v1.0"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/ping");
            });
        }
    }
}
