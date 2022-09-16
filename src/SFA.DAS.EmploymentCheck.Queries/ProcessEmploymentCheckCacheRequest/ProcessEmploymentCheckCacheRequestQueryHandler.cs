using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries.ProcessEmploymentCheckCacheRequest
{
    public class ProcessEmploymentCheckCacheRequestQueryHandler : IQueryHandler<ProcessEmploymentCheckCacheRequestQueryRequest, ProcessEmploymentCheckCacheRequestQueryResult>
    {
        private readonly IEmploymentCheckService _service;

        public ProcessEmploymentCheckCacheRequestQueryHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<ProcessEmploymentCheckCacheRequestQueryResult> Handle(ProcessEmploymentCheckCacheRequestQueryRequest request, CancellationToken cancellationToken = default)
        {
            var employmentCheckCacheRequests = await _service.GetEmploymentCheckCacheRequests();

            return new ProcessEmploymentCheckCacheRequestQueryResult(employmentCheckCacheRequests);
        }
    }
}
