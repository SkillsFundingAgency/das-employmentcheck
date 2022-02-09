using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch();

        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<IList<EmploymentCheckCacheRequest>> SetCacheRequestRelatedRequestsProcessingStatus(Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet);

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task<long> StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest);
    }
}
