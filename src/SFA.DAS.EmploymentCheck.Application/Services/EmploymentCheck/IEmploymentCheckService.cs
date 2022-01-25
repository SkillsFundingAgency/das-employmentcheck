using System;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        Task<IList<Data.Models.EmploymentCheck>> GetEmploymentChecksBatch();

        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest);

        public Task<Data.Models.EmploymentCheck> GetLastEmploymentCheck(Guid correlationId);

        public void InsertEmploymentCheck(Data.Models.EmploymentCheck employmentCheck);
    }
}