using MediatR;

namespace SFA.DAS.EmploymentCheck.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryRequest
        : IRequest<GetEmploymentCheckBatchQueryResult>
    {
        public GetEmploymentCheckBatchQueryRequest()
        {
        }
    }
}