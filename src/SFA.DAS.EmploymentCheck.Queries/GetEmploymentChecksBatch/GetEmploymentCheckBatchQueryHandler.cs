using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Clients.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryHandler
        : IRequestHandler<GetEmploymentCheckBatchQueryRequest,
            GetEmploymentCheckBatchQueryResult>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public GetEmploymentCheckBatchQueryHandler(
            ILogger<GetEmploymentCheckBatchQueryHandler> logger,
            IEmploymentCheckClient employmentCheckClient)
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<GetEmploymentCheckBatchQueryResult> Handle(
            GetEmploymentCheckBatchQueryRequest request,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(request, nameof(request));

            var employmentChecks = await _employmentCheckClient.GetEmploymentChecksBatch();

            return new GetEmploymentCheckBatchQueryResult(employmentChecks);
        }
    }
}
