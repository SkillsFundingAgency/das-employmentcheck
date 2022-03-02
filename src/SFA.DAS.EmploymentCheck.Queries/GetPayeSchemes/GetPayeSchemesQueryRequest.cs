using MediatR;

namespace SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes
{
    public class GetPayeSchemesQueryRequest
        : IRequest<GetPayeSchemesQueryResult>
    {
        public GetPayeSchemesQueryRequest(Data.Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Data.Models.EmploymentCheck EmploymentCheck { get; }
    }
}

