using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest
{
    public class ProcessEmploymentCheckCacheRequestQueryResult
    {
        public ProcessEmploymentCheckCacheRequestQueryResult(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
