using MediatR;

namespace SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber
{
    public class GetDbNiNumberQueryRequest
        : IRequest<GetDbNiNumberQueryResult>
    {
        public GetDbNiNumberQueryRequest(Data.Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Data.Models.EmploymentCheck EmploymentCheck { get; }
    }
}
