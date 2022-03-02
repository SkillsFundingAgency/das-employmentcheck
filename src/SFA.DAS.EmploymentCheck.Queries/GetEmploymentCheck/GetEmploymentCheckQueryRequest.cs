using MediatR;

namespace SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck
{
    public class GetEmploymentCheckQueryRequest
        : IRequest<GetEmploymentCheckQueryResult>
    {
        public GetEmploymentCheckQueryRequest()
        {
        }
    }
}