using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.ProcessEmploymentCheckCacheRequest
{
    public class ProcessEmploymentCheckCacheRequestQueryResult
    {
        public ProcessEmploymentCheckCacheRequestQueryResult(EmploymentCheckCacheRequest[] employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest[] EmploymentCheckCacheRequest { get; }
    }
}
