using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        Task<Models.EmploymentCheck> GetEmploymentCheck();

        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task StoreCompletedCheck(EmploymentCheckCacheRequest request, EmploymentCheckCacheResponse response);

        Task InsertEmploymentCheckCacheResponse(EmploymentCheckCacheResponse response);
    }
}