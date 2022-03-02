using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumber
{
    public class GetNiNumberQueryRequest
        : IRequest<GetNiNumberQueryResult>
    {
        public GetNiNumberQueryRequest(Application.Models.EmploymentCheck check)
        {
            Check = check;
        }

        public Application.Models.EmploymentCheck Check { get; }
    }
}
