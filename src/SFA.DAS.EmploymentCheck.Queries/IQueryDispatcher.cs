using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries
{
    public interface IQueryDispatcher
    {
        Task<TResult> Send<TQuery, TResult>(TQuery query) where TQuery : IQuery;
    }

    public interface IQuery
    {
    }
}
