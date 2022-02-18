using System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Scrutor;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequests;
using SFA.DAS.EmploymentCheck.Commands.StoreEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Commands
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMediatR(typeof(CreateEmploymentCheckCacheCommand).Assembly);
            serviceCollection.AddMediatR(typeof(StoreEmploymentCheckResultCommand).Assembly);

            // set up the command handlers and command validators
            serviceCollection.Scan(scan =>
            {
                scan.FromAssembliesOf(typeof(ServiceCollectionExtensions))
                    .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime();

                //.AddClasses(classes => classes.AssignableTo(typeof(IValidator<>)))
                //.AsImplementedInterfaces()
                //.WithSingletonLifetime();
            });

            //if (addDecorators != null)
            //{
            //    serviceCollection = addDecorators(serviceCollection);
            //}

            return serviceCollection;
        }
    }
}
