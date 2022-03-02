using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SFA.DAS.Api.Common.AppStart;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.Api.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using PolicyNames = SFA.DAS.Api.Common.Infrastructure.PolicyNames;

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
            services.AddApplicationInsightsTelemetry();
            services.AddHealthChecks();
            services.AddNLogForApi();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SFA.DAS.EmploymentCheck.Api", Version = "v1.0"});
                c.OperationFilter<AddVersionHeaderParameter>();
            });
       
            services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));

            services.Configure<EmploymentCheckSettings>(Configuration.GetSection("ApplicationSettings"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EmploymentCheckSettings>>().Value);

            if (!Configuration["EnvironmentName"].Equals("DEV", StringComparison.CurrentCultureIgnoreCase) &&
                !Configuration["EnvironmentName"].Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
            {
                services.AddSingleton(new AzureServiceTokenProvider());

                var azureAdConfiguration = Configuration
                    .GetSection("AzureAd")
                    .Get<AzureActiveDirectoryConfiguration>();
                
                var policies = new Dictionary<string, string>
                {
                    { "default", PolicyNames.Default }
                };

                services.AddAuthentication(azureAdConfiguration, policies);

                services
                    .AddMvc(o => { o.Conventions.Add(new AuthorizeControllerModelConvention(new List<string>())); })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            }

            services
                .AddRepositories()
                .AddServices()
                .AddHandlers()
                ;

            services.AddApiVersioning(opt =>
            {
                opt.ApiVersionReader = new HeaderApiVersionReader("X-Version");
            });


        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/ping");
            });

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SFA.DAS.EmploymentCheck.Api v1.0"));
        }
    }
}
