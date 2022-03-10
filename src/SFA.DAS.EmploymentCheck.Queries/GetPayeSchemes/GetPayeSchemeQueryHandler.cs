using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes
{
    public class GetPayeSchemeQueryHandler
        : IRequestHandler<GetPayeSchemesQueryRequest,
            GetPayeSchemesQueryResult>
    {
        private readonly IEmployerAccountClient _employerAccountClient;

        public GetPayeSchemeQueryHandler(
            IEmployerAccountClient employerAccountClient)
        {
            _employerAccountClient = employerAccountClient;
        }

        public async Task<GetPayeSchemesQueryResult> Handle(
            GetPayeSchemesQueryRequest getPayeSchemesRequest,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(getPayeSchemesRequest, nameof(getPayeSchemesRequest));
            Guard.Against.Null(getPayeSchemesRequest.EmploymentCheck, nameof(getPayeSchemesRequest.EmploymentCheck));

            var result = await _employerAccountClient.GetEmployersPayeSchemes(getPayeSchemesRequest.EmploymentCheck);

            return new GetPayeSchemesQueryResult(result);
        }
    }
}