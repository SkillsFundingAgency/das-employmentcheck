using MediatR;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryRequest
        : IRequest<GetEmploymentCheckBatchQueryResult>
    {
        public GetEmploymentCheckBatchQueryRequest()
        {
        }
    }
}