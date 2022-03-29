using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessEmploymentCheckRequestsOrchestrator
    {
        private readonly ILogger<ProcessEmploymentCheckRequestsOrchestrator> _logger;

        public ProcessEmploymentCheckRequestsOrchestrator(
            ILogger<ProcessEmploymentCheckRequestsOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(ProcessEmploymentCheckRequestsOrchestrator))]
        public async Task ProcessEmploymentCheckRequestsOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            EmploymentCheckCacheRequest request;

            do
            {
                request = await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null);
                
                await ProcessEmploymentCheckRequest(context, request);

            } while (request != null);

            _logger.LogInformation($"\n\n{nameof(ProcessEmploymentCheckRequestsOrchestrator)}: {nameof(GetEmploymentCheckCacheRequestActivity)} returned no results. Nothing to process.");
        }

        private static async Task ProcessEmploymentCheckRequest(IDurableOrchestrationContext context, EmploymentCheckCacheRequest request)
        {
            if (request == null) return;

            await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), request);
        }
    }
}