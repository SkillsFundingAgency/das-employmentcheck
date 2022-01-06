using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes
{
    public class GetPayeSchemesQueryRequest
        : IRequest<GetPayeSchemesQueryResult>
    {
        public GetPayeSchemesQueryRequest(IList<Application.Models.EmploymentCheck> employmentCheckBatch)
        {
            EmploymentCheckBatch = employmentCheckBatch;
        }

        public IList<Application.Models.EmploymentCheck> EmploymentCheckBatch { get; }
    }
}

