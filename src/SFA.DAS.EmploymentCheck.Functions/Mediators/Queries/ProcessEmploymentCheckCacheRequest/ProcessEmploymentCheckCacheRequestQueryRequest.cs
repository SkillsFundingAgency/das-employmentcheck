using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest
{
    public class ProcessEmploymentCheckCacheRequestQueryRequest
        : IRequest<ProcessEmploymentCheckCacheRequestQueryResult>
    {
        public ProcessEmploymentCheckCacheRequestQueryRequest() { }
    }
}
