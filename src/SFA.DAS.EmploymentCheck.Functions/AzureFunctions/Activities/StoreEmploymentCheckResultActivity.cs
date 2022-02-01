using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class StoreEmploymentCheckResultActivity
    {
        private readonly IMediator _mediator;

        public StoreEmploymentCheckResultActivity(
            ILogger<StoreEmploymentCheckResultActivity> logger,
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(StoreEmploymentCheckResultActivity))]
        public async Task StoreEmploymentCheckResultTask([ActivityTrigger] EmploymentCheckCacheRequest request)
        {
            await _mediator.Send(new StoreEmploymentCheckResultCommand(request));
        }
    }
}