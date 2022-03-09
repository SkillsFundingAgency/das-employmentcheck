using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Queries
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryServices(this IServiceCollection serviceCollection)
        {
            serviceCollection
                .Scan(scan =>
                {
                    scan.FromExecutingAssembly()
                        .AddClasses(classes => classes.AssignableTo(typeof(IQuery)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();

                    scan.FromAssembliesOf(typeof(GetEmploymentCheckQueryHandler))
                        .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();
                })
                .AddScoped<IQueryDispatcher, QueryDispatcher>()
                ;

            return serviceCollection;
        }
    }
}
