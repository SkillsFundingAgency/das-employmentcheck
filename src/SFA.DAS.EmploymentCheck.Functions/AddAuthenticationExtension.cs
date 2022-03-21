using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Api.Common.Configuration;
using SFA.DAS.Api.Common.Infrastructure;
using SFA.DAS.Api.Common.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public static class AddAuthenticationExtension
    {
        public static void AddAuthentication(this IServiceCollection services, AzureActiveDirectoryConfiguration config)
        {
            services.AddAuthorization(o =>
            {
                o.AddPolicy(PolicyNames.Default, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("Default");
                });
            });

            services.AddAuthentication(auth => { auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; })
                .AddJwtBearer(auth =>
                {
                    auth.Authority =
                        $"https://login.microsoftonline.com/{config.Tenant}";
                    auth.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidAudiences = new List<string>
                        {
                            config.Identifier
                        }
                    };
                });
            services.AddSingleton<IClaimsTransformation, AzureAdScopeClaimTransformation>();
        }
    }
}
