using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck
{
    public class GetEmploymentCheckQueryRequest
        : IRequest<GetEmploymentCheckQueryResult>
    {
        public GetEmploymentCheckQueryRequest()
        {
        }
    }
}