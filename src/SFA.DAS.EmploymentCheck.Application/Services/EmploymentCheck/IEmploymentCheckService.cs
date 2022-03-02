using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck
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