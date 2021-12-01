using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecks
{
    public class GetEmploymentChecksQueryRequest
        : IRequest<GetEmploymentChecksQueryResult>
    {
        public GetEmploymentChecksQueryRequest(long employmentCheckLastHighestBatchId)
        {
            EmploymentCheckLastHighestBatchId = employmentCheckLastHighestBatchId;
        }

        public long EmploymentCheckLastHighestBatchId { get; }
    }
}