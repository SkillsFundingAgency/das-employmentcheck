using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        /// <inheritdoc>
        /// <summary>
        /// Gets a batch of the apprentices requiring employment checks from the Employment Check database.
        /// </summary>
        /// <returns>IList<ApprenticeEmploymentCheck></returns>
        Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch();

        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given batch of employment checks
        /// </summary>
        /// <param name="employmentCheckBatch"></param>
        /// <returns></returns>
        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task StoreEmploymentCheckCacheRequest(EmploymentCheckCacheRequest employmentCheckCacheRequest);

        Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest);

        Task SeedEmploymentCheckDatabaseTableTestData();
    }
}
