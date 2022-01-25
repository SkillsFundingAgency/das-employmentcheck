using MediatR;

namespace SFA.DAS.EmploymentCheck.Queries.ProcessEmploymentCheckCacheRequest
{
    public class ProcessEmploymentCheckCacheRequestQueryRequest
        : IRequest<ProcessEmploymentCheckCacheRequestQueryResult>
    {
        public ProcessEmploymentCheckCacheRequestQueryRequest() { }
    }
}
