using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries
{
    public interface IQueryDispatcher
    {
        Task<TResult> Send<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery;
    }
}
