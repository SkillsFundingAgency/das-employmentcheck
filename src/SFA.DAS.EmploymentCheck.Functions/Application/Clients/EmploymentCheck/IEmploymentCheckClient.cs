using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch();

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest);

        Task UpdateRequestCompletionStatusForRelatedEmploymentCheckCacheRequests(Models.EmploymentCheckCacheRequest request);
    }
}
