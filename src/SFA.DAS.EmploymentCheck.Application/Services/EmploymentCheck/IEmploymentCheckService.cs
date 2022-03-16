using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        Task<Data.Models.EmploymentCheck> GetEmploymentCheck();

        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task StoreCompletedCheck(EmploymentCheckCacheRequest request, EmploymentCheckCacheResponse response);

        Task InsertEmploymentCheckCacheResponse(EmploymentCheckCacheResponse response);

        Task SaveEmploymentCheck(Data.Models.EmploymentCheck check);
    }
}