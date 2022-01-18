using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.Api.Application.Services;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;
using SFA.DAS.EmploymentCheck.Api.Repositories;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            services.AddMediatR(typeof(StoreEmploymentCheckResultCommand).Assembly);
            services.AddMediatR(typeof(RegisterCheckCommand).Assembly);
            services.AddMediatR(typeof(CreateEmploymentCheckCacheCommand).Assembly);

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddTransient<IEmploymentCheckClient, EmploymentCheckClient>();
            services.AddTransient<IEmploymentCheckService, EmploymentCheckService>();
            services.AddTransient<IRegisterCheckCommandValidator, RegisterCheckCommandValidator>();
            
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IEmploymentCheckRepository, EmploymentCheckRepository>();

            return services;
        }
    }
}
