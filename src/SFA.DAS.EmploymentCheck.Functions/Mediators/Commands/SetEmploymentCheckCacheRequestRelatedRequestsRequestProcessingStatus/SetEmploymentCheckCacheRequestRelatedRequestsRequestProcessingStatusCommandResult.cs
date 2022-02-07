using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus
{
    public class SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommandResult
    {
        public SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommandResult(
            IList<EmploymentCheckCacheRequest> employmentCheckCacheRequests
        )
        {
            EmploymentCheckCacheRequests = employmentCheckCacheRequests;
        }

        public IList<EmploymentCheckCacheRequest> EmploymentCheckCacheRequests { get; }
    }
}

