using MediatR;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetNextEmploymentCheckCacheRequest
{
    public class GetNextEmploymentCheckCacheRequestQueryRequest
        : IRequest<GetNextEmploymentCheckCacheRequestQueryResult>
    {
        public GetNextEmploymentCheckCacheRequestQueryRequest() { }
    }
}
