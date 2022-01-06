using MediatR;

namespace SFA.DAS.EmploymentCheck.Application.ApprenticeEmploymentChecks.Queries.GetApprenticeEmploymentChecks.Models
{
    public class GetEmploymentChecksQueryRequest
        : IRequest<GetEmploymentCheckQueryResult>
    {
        public GetEmploymentChecksQueryRequest(long lastHighestBatchId)
        {
            LastHighestBatchId = lastHighestBatchId;
        }

        public long LastHighestBatchId { get; }
    }
}