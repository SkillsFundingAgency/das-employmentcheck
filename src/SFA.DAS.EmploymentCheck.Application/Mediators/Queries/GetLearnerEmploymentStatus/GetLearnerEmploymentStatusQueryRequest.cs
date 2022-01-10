using MediatR;
using SFA.DAS.EmploymentCheck.Domain.Entities;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetLearnerEmploymentStatus
{
    public class GetLearnerEmploymentStatusQueryRequest
        : IRequest<GetLearnerEmploymentStatusQueryResult>
    {
        public GetLearnerEmploymentStatusQueryRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
