using System;
using System.Threading.Tasks;
using Dapper;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        Task BeginAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task UpdateAsync<T>(T entity) where T : class;
        Task InsertAsync<T>(T entity) where T : class;
        Task ExecuteSqlAsync(string sql, DynamicParameters parameter = null);
    }
}