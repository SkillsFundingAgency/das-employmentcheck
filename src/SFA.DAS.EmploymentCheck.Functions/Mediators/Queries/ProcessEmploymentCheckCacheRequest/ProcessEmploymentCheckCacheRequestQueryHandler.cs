using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest
{
    public class ProcessEmploymentCheckCacheRequestQueryHandler
        : IRequestHandler<ProcessEmploymentCheckCacheRequestQueryRequest,
            ProcessEmploymentCheckCacheRequestQueryResult>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public ProcessEmploymentCheckCacheRequestQueryHandler(
            ILogger<ProcessEmploymentCheckCacheRequestQueryHandler> logger,
            IEmploymentCheckClient employmentCheckMessageQueueClient)
        {
            _employmentCheckClient = employmentCheckMessageQueueClient;
        }

        public async Task<ProcessEmploymentCheckCacheRequestQueryResult> Handle(
            ProcessEmploymentCheckCacheRequestQueryRequest request,
            CancellationToken cancellationToken)
        {
            var employmentCheckCacheRequest = await _employmentCheckClient.ProcessEmploymentCheckCacheRequest();

            return new ProcessEmploymentCheckCacheRequestQueryResult(employmentCheckCacheRequest);
        }
    }
}
