using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.EmploymentCheck.Queries
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<TResult> Send<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery
        {
            var service = _serviceProvider.GetService<IQueryHandler<TQuery, TResult>>();

            if (service == null)
            {
                throw new QueryDispatcherException($"Unable to dispatch query '{query.GetType().Name}'. No matching handler found.");
            }

            try
            {
                return service.Handle(query, cancellationToken);
            }
            catch (Exception e)
            {
                throw new QueryException($"Unable to execute query '{query.GetType().Name}'.", e);
            }
        }
    }
}