using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryRequest
        : IRequest<GetHmrcLearnerEmploymentStatusQueryResult>
    {
        public GetHmrcLearnerEmploymentStatusQueryRequest(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
