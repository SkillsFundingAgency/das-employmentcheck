using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks
{
    public class GetApprenticeEmploymentChecksQueryRequest
        : IRequest<GetApprenticeEmploymentChecksQueryResult>
    {
        public GetApprenticeEmploymentChecksQueryRequest(long employmentCheckLastGetId)
        {
            employmentCheckLastGetId = employmentCheckLastGetId;
        }

        public long employmentCheckLastGetId { get; }
    }
}