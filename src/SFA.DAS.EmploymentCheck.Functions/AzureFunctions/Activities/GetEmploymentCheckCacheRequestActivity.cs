using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Queries.ProcessEmploymentCheckCacheRequest;

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
        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckRequestActivityTask(
            [ActivityTrigger] object input)
        {
            //var processEmploymentCheckCacheRequestQueryResult = await _mediator.Send(new ProcessEmploymentCheckCacheRequestQueryRequest());

            EmploymentCheckCacheRequest processEmploymentCheckCacheRequestQueryResult = null;

            return processEmploymentCheckCacheRequestQueryResult;
        }
    }
}