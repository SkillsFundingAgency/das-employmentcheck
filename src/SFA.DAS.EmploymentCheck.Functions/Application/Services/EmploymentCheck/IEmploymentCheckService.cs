using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch();

        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest);
    }
}