using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentCheckCacheRequestActivity
    {
        private readonly IMediator _mediator;

        public GetEmploymentCheckCacheRequestActivity(
            IMediator mediator,
            ILogger<GetEmploymentCheckCacheRequestActivity> logger)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(GetEmploymentCheckCacheRequestActivity))]
        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequestActivityTask(
            [ActivityTrigger] object input)
        {
            var processEmploymentCheckCacheRequestQueryResult = await _mediator.Send(new ProcessEmploymentCheckCacheRequestQueryRequest());

            return processEmploymentCheckCacheRequestQueryResult?.EmploymentCheckCacheRequest;
        }
    }
}