using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ICommandDispatcher, CommandDispatcher>();

            serviceCollection.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();
            });

            return serviceCollection;
        }
    }
}
