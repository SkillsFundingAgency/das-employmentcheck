using MediatR;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus
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
