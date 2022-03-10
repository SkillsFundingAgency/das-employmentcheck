using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

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
            var thisMethodName = $"{nameof(ProcessEmploymentCheckRequestsOrchestrator)}.ProcessEmploymentCheckRequestsOrchestratorTask";

            try
            {
                // Get the next request
                var employmentCheckCacheRequest = await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null);

                if (employmentCheckCacheRequest != null)
                {
                    context.SetCustomStatus("Processing");

                    // Do the employment status check on this request
                    await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), employmentCheckCacheRequest);
                }
                else
                {
                    context.SetCustomStatus("Idle");
                   
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(GetEmploymentCheckCacheRequestActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    var sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"\n\n{nameof(ProcessEmploymentCheckRequestsOrchestrator)} Exception caught: {e.Message}. {e.StackTrace}");
            }
            finally
            {
                context.ContinueAsNew(null);
            }

        }
    }
}