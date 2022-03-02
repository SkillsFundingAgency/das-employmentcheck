using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes
{
    public class GetPayeSchemesQueryRequest
        : IRequest<GetPayeSchemesQueryResult>
    {
        public GetPayeSchemesQueryRequest(Application.Models.EmploymentCheck employmentCheck)
        {
            EmploymentCheck = employmentCheck;
        }

        public Application.Models.EmploymentCheck EmploymentCheck { get; }
    }
}

