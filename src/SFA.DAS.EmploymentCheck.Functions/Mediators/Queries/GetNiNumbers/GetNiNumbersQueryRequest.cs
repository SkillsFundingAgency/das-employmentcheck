using MediatR;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers
{
    public class GetNiNumbersQueryRequest
        : IRequest<GetNiNumbersQueryResult>
    {
        public GetNiNumbersQueryRequest(Application.Models.EmploymentCheck check)
        {
            Check = check;
        }

        public Application.Models.EmploymentCheck Check { get; }
    }
}
