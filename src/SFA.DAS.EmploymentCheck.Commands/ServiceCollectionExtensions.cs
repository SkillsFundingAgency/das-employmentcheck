using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.Abstractions;
using System;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection, Func<IServiceCollection, IServiceCollection> addDecorators = null)
        {
            serviceCollection.AddScoped<ICommandDispatcher, CommandDispatcher>();

            serviceCollection.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();
            });

            serviceCollection.AddScoped<ICommandPublisher, CommandPublisher>();

            if (addDecorators != null)
            {
                serviceCollection = addDecorators(serviceCollection);
            }

            return serviceCollection;
        }
    }
}
