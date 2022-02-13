using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch();

        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData, IEmploymentCheckCacheRequestFactory cacheRequestFactory);

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task<IList<EmploymentCheckCacheRequest>> SetCacheRequestRelatedRequestsProcessingStatus(Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet);

        Task<long> StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest);
    }
}