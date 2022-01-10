
using SFA.DAS.EmploymentCheck.Domain.Entities;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetLearnerEmploymentStatus
{
    public class GetLearnerEmploymentStatusQueryResult
    {
        public GetLearnerEmploymentStatusQueryResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
