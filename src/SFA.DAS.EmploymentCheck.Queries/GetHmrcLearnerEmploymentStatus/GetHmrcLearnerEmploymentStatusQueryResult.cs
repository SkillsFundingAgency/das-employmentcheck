using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryResult
    {
        public GetHmrcLearnerEmploymentStatusQueryResult() { }

        public GetHmrcLearnerEmploymentStatusQueryResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; set;  }
    }
}
