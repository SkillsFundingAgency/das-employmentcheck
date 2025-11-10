using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.EmploymentCheck.Api.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SFA.DAS.EmploymentCheck.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            var builder = new ConfigurationBuilder()
                .AddConfiguration(configuration)
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddEnvironmentVariables();

            builder.AddJsonFile("appsettings.Development.json", optional: true);

            builder.AddAzureTableStorage(options =>
            {
                options.ConfigurationKeys = configuration["ConfigNames"]?.Split(",") ?? Array.Empty<string>();
                options.StorageConnectionString = configuration["ConfigurationStorageConnectionString"];
                options.EnvironmentName = configuration["EnvironmentName"];
                options.PreFixConfigurationKeys = false;
            });

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks();
            services.AddNLogForApi();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SFA.DAS.EmploymentCheck.Api", Version = "v1.0" });
                c.CustomSchemaIds(type => type.FullName);
            });

            services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), Configuration));

            services.Configure<EmploymentCheckSettings>(Configuration.GetSection("ApplicationSettings"));
            services.AddSingleton(cfg => cfg.GetService<IOptions<EmploymentCheckSettings>>().Value);

            var envName = Configuration["EnvironmentName"] ?? string.Empty;
            if (!envName.Equals("LOCAL", StringComparison.OrdinalIgnoreCase))
            {
                var tenant = Configuration["AzureAd:Tenant"] ?? string.Empty;
                var identifierUri = Configuration["AzureAd:Identifier"];
                var clientId = Configuration["AzureAd:ClientId"];

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = $"https://login.microsoftonline.com/{tenant}/v2.0";

                    var audiences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    if (!string.IsNullOrWhiteSpace(identifierUri))
                    {
                        audiences.Add(identifierUri);

                        if (identifierUri.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                            && !identifierUri.EndsWith("-ar", StringComparison.OrdinalIgnoreCase))
                        {
                            audiences.Add($"{identifierUri}-ar");
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(clientId))
                    {
                        audiences.Add($"api://{clientId}");
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidAudiences = audiences
                    };
                });

                services.AddAuthorization(o =>
                {
                    o.AddPolicy(PolicyNames.Default, p => p.RequireAuthenticatedUser());
                });

                services.AddMvc(o =>
                {
                    o.Conventions.Add(new AuthorizeControllerModelConvention(new List<string>()));
                }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
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
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

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
