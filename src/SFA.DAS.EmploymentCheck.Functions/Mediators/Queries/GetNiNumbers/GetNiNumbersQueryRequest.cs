using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers
{
    public class GetNiNumbersQueryRequest
        : IRequest<GetNiNumbersQueryResult>
    {
        public GetNiNumbersQueryRequest(IList<Application.Models.EmploymentCheck> employmentCheckBatch)
        {
            EmploymentCheckBatch = employmentCheckBatch;
        }

        public IList<Application.Models.EmploymentCheck> EmploymentCheckBatch { get; }
    }
}
