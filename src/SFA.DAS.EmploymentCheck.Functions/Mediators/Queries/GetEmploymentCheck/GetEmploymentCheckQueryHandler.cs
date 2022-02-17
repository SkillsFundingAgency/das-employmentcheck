using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck
{
    public class GetEmploymentCheckQueryHandler
        : IRequestHandler<GetEmploymentCheckQueryRequest,
            GetEmploymentCheckQueryResult>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public GetEmploymentCheckQueryHandler(
            ILogger<GetEmploymentCheckQueryHandler> logger,
            IEmploymentCheckClient employmentCheckClient)
        {
            _employmentCheckClient = employmentCheckClient;

        }

        public async Task<GetEmploymentCheckQueryResult> Handle(
            GetEmploymentCheckQueryRequest request,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(request, nameof(request));

            var employmentChecks = await _employmentCheckClient.GetEmploymentCheck();

            return new GetEmploymentCheckQueryResult(employmentChecks);
        }
    }
}
