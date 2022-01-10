
using SFA.DAS.EmploymentCheck.Domain.Entities;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetNextEmploymentCheckCacheRequest
{
    public class GetNextEmploymentCheckCacheRequestQueryResult
    {
        public GetNextEmploymentCheckCacheRequestQueryResult(EmploymentCheckCacheRequest request)
        {
            EmploymentCheckCacheRequest = request;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
