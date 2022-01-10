using MediatR;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetPayeSchemes
{
    public class GetPayeSchemesQueryRequest
        : IRequest<GetPayeSchemesQueryResult>
    {
        public GetPayeSchemesQueryRequest(IList<Domain.Entities.EmploymentCheck> employmentCheckBatch)
        {
            EmploymentCheckBatch = employmentCheckBatch;
        }

        public IList<Domain.Entities.EmploymentCheck> EmploymentCheckBatch { get; }
    }
}

