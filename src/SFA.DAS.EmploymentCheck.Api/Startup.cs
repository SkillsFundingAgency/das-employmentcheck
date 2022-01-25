using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.IO;

namespace SFA.DAS.EmploymentCheck.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SFA.DAS.EmploymentCheck.Api", Version = "v1.0"});
            });

            var configBuilder = new ConfigurationBuilder()
                .AddConfiguration(Configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            if (Configuration["Environment"] != "Development")
            {
                configBuilder.AddAzureTableStorage(options =>
                {
                    options.ConfigurationKeys = Configuration["ConfigNames"].Split(",");
                    options.StorageConnectionString = Configuration["ConfigurationStorageConnectionString"];
                    options.EnvironmentName = Configuration["EnvironmentName"];
                    options.PreFixConfigurationKeys = false;
                });
            }

            configBuilder.AddJsonFile("appsettings.Development.json", optional: true);

            var config = configBuilder.Build();
            services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            // Application Configuration
            services.Configure<ApplicationSettings>(config.GetSection("ApplicationSettings"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<ApplicationSettings>>().Value);

            if (Configuration["Environment"] != "Development")
            {
                services.AddSingleton(new AzureServiceTokenProvider());
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

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/ping");
            });
        }
    }
}
