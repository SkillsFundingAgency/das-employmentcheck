using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryRequest
        : IRequest<GetEmploymentCheckBatchQueryResult>
    {
        public GetEmploymentCheckBatchQueryRequest()
        {
        }
    }
}