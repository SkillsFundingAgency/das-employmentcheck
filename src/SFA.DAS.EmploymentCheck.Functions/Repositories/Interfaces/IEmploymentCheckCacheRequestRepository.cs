using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckCacheRequestRepository
    {
        Task Save(EmploymentCheckCacheRequest employmentCheckCacheRequest);

        Task<IList<EmploymentCheckCacheRequest>> SetRelatedRequestsCompletionStatus(Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet);

        Task UpdateEmployedAndRequestStatusFields(EmploymentCheckCacheRequest employmentCheckCacheRequest);

        Task<EmploymentCheckCacheRequest> GetNext();
    }
}
