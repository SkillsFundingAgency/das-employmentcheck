using MediatR;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes
{
    public class GetEmployerPayeSchemesRequest
        : IRequest<GetEmployerPayeSchemesResult>
    {
        public GetEmployerPayeSchemesRequest(IList<long> accountIds)
        {
            AccountIds = accountIds;
        }

        public IList<long> AccountIds { get; }
    }
}

