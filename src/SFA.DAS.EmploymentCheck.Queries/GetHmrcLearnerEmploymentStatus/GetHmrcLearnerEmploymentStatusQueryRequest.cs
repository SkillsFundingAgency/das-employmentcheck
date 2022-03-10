using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryRequest : IQuery
    {
        public GetHmrcLearnerEmploymentStatusQueryRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
