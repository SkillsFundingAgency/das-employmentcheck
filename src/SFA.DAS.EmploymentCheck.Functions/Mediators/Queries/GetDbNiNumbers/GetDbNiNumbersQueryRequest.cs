using MediatR;
using System.Collections.Generic;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumbers
{
    public class GetDbNiNumbersQueryRequest
        : IRequest<GetDbNiNumbersQueryResult>
    {
        public GetDbNiNumbersQueryRequest(IList<Models.EmploymentCheck> employmentCheckBatch)
        {
            EmploymentCheckBatch = employmentCheckBatch;
        }

        public IList<Models.EmploymentCheck> EmploymentCheckBatch { get; }
    }
}
