using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus
{
    public class SetCacheRequestRelatedRequestsProcessingStatusCommandResult
    {
        public SetCacheRequestRelatedRequestsProcessingStatusCommandResult(
            IList<EmploymentCheckCacheRequest> employmentCheckCacheRequests
        )
        {
            EmploymentCheckCacheRequests = employmentCheckCacheRequests;
        }

        public IList<EmploymentCheckCacheRequest> EmploymentCheckCacheRequests { get; }
    }
}

