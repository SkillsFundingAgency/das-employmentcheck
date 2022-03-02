using MediatR;

namespace SFA.DAS.EmploymentCheck.Queries.GetNiNumber
{
    public class GetNiNumberQueryRequest
        : IRequest<GetNiNumberQueryResult>
    {
        public GetNiNumberQueryRequest(Data.Models.EmploymentCheck check)
        {
            Check = check;
        }

        public Data.Models.EmploymentCheck Check { get; }
    }
}
