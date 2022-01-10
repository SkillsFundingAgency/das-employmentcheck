using MediatR;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetNiNumbers
{
    public class GetNiNumbersQueryRequest
        : IRequest<GetNiNumbersQueryResult>
    {
        public GetNiNumbersQueryRequest(IList<Domain.Entities.EmploymentCheck> employmentCheckBatch)
        {
            EmploymentCheckBatch = employmentCheckBatch;
        }

        public IList<Domain.Entities.EmploymentCheck> EmploymentCheckBatch { get; }
    }
}
