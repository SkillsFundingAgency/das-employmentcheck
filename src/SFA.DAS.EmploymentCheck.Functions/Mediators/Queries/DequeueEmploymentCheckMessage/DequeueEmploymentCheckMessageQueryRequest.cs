using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueEmploymentCheckMessage
{
    public class DequeueEmploymentCheckMessageQueryRequest
        : IRequest<DequeueEmploymentCheckMessageQueryResult>
    {
        public DequeueEmploymentCheckMessageQueryRequest() { }
    }
}
