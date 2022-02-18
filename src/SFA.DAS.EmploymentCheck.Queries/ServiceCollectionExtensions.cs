using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentChecksBatch;
using SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus;
using SFA.DAS.EmploymentCheck.Queries.GetNiNumbers;
using SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes;
using SFA.DAS.EmploymentCheck.Queries.ProcessEmploymentCheckCacheRequest;

namespace SFA.DAS.EmploymentCheck.Queries
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddQueryServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMediatR(typeof(ProcessEmploymentCheckCacheRequestQueryRequest).Assembly);
            // serviceCollection.AddMediatR(typeof(GetEmploymentCheckBatchQueryRequest).Assembly);
            serviceCollection.AddMediatR(typeof(GetHmrcLearnerEmploymentStatusQueryRequest).Assembly);
            serviceCollection.AddMediatR(typeof(GetNiNumbersQueryRequest).Assembly);
            serviceCollection.AddMediatR(typeof(GetPayeSchemesQueryRequest).Assembly);

            serviceCollection
                .Scan(scan =>
                {
                    scan.FromExecutingAssembly()
                        .AddClasses(classes => classes.AssignableTo(typeof(IQuery)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();

                    scan.FromAssembliesOf(typeof(GetEmploymentCheckBatchQueryHandler))
                        .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                        .AsImplementedInterfaces()
                        .WithTransientLifetime();

                    //scan.FromAssembliesOf(typeof(AccountQueryRepository))
                    //    .AddClasses(classes => classes.AssignableTo(typeof(IQueryRepository<>)))
                    //    .AsImplementedInterfaces()
                    //    .WithTransientLifetime();
                })
                .AddScoped<IQueryDispatcher, QueryDispatcher>()
                ;

            return serviceCollection;
        }
    }
}
