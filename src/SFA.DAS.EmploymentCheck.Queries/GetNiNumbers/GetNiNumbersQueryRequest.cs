using System.Collections.Generic;
using MediatR;

namespace SFA.DAS.EmploymentCheck.Queries.GetNiNumbers
{
    public class GetNiNumbersQueryRequest
        : IRequest<GetNiNumbersQueryResult>
    {
        public GetNiNumbersQueryRequest(IList<Data.Models.EmploymentCheck> employmentCheckBatch)
        {
            EmploymentCheckBatch = employmentCheckBatch;
        }

        public IList<Data.Models.EmploymentCheck> EmploymentCheckBatch { get; }
    }
}
