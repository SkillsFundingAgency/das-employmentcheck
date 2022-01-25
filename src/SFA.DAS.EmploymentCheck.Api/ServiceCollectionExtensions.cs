using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequests;
using SFA.DAS.EmploymentCheck.Commands.RegisterCheck;
using SFA.DAS.EmploymentCheck.Commands.StoreEmploymentCheckResult;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;

namespace SFA.DAS.EmploymentCheck.Api
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            services.AddMediatR(typeof(RegisterCheckCommand).Assembly);
            services.AddMediatR(typeof(StoreEmploymentCheckResultCommand).Assembly);
            services.AddMediatR(typeof(CreateEmploymentCheckCacheCommand).Assembly);
            
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddTransient<IEmploymentCheckService, EmploymentCheckService>();
            services.AddTransient<IRegisterCheckCommandValidator, RegisterCheckCommandValidator>();
            
            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddTransient<IEmploymentCheckRepository, EmploymentCheckRepository>();
            services.AddTransient<IEmploymentCheckCacheRequestRepository, EmploymentCheckCacheRequestRepository>();

            return services;
        }
    }
}
