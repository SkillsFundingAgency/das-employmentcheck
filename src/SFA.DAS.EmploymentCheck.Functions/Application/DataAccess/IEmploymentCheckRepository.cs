using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.DataAccess
{
    public interface IEmploymentCheckRepository<T>
    {
#pragma warning disable CS0693 // Type parameter has the same name as the type parameter from outer type
        Task<int> Insert<T>(T entity);
#pragma warning restore CS0693 // Type parameter has the same name as the type parameter from outer type

        //Task<IList<T>> GetAllAsync();

        //Task<T> GetByIdAsync(long id);

        //Task<int> InsertAsync(T entity);

        //Task<int> UpdateAsync(T entity);

        //Task<int> DeleteAsyn(T entity);


        /// <inheritdoc>
        /// <summary>
        /// Gets a batch of the apprentices requiring employment checks from the Employment Check database.
        /// </summary>
        /// <returns>IList<EmploymentCheckModel></returns>
        Task<IList<EmploymentCheckModel>> GetEmploymentCheckBatchById(long employmentCheckLastHighestBatchId, long batchSize);

        /// <summary>
        /// Gets the highest employment check id from the last batch of employment checks on the message queue
        /// </summary>
        /// <param name="sqlConnection"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<long> GetEmploymentCheckLastHighestBatchId(SqlConnection sqlConnection, SqlTransaction transaction);

        Task<int> StoreEmploymentCheckRequest(EmploymentCheckCacheRequest employmentCheckCacheRequest);

        Task<int> StoreEmploymentCheckMessage(EmploymentCheckMessage employmentCheckMessage);
    }
}