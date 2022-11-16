using SFA.DAS.EmploymentCheck.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        Task<Data.Models.EmploymentCheck> GetEmploymentCheck();

        Task<Data.Models.EmploymentCheck> GetResponseEmploymentCheck();

        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<EmploymentCheckCacheRequest[]> GetEmploymentCheckCacheRequests();

        Task StoreCompletedEmploymentCheck(Models.EmploymentCheck employmentCheck);

        Task StoreCompletedCheck(EmploymentCheckCacheRequest request, EmploymentCheckCacheResponse response);

        Task InsertEmploymentCheckCacheResponse(EmploymentCheckCacheResponse response);

        Task SaveEmploymentCheck(Data.Models.EmploymentCheck check);

        Task AbandonRelatedRequests(EmploymentCheckCacheRequest[] employmentCheckCacheRequests);
    }
}