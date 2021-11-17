using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueApprenticeEmploymentCheckMessage
{
    public class DequeueApprenticeEmploymentCheckMessageQueryRequest
        : IRequest<DequeueApprenticeEmploymentCheckMessageQueryResult>
    {
        public DequeueApprenticeEmploymentCheckMessageQueryRequest() { }
    }
}
