using MediatR;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumber
{
    public class GetDbNiNumberQueryRequest
        : IRequest<GetDbNiNumberQueryResult>
    {
        public GetDbNiNumberQueryRequest(Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Models.EmploymentCheck EmploymentCheck { get; }
    }
}
