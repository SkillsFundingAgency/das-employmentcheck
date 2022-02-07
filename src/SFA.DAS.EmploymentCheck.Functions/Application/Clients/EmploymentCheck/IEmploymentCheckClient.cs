using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData);

        Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch();

        Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest();

        Task<IList<EmploymentCheckCacheRequest>> SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus(Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet);

        Task<EmploymentCheckCacheRequest> ProcessEmploymentCheckCacheRequest();

        Task UpdateRequestCompletionStatusForRelatedEmploymentCheckCacheRequests(Models.EmploymentCheckCacheRequest request);
    }
}
