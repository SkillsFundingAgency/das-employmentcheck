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
        public async Task ProcessEmploymentChecksWithRateLimiterOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{nameof(ProcessEmploymentCheckRequestsOrchestrator)}.ProcessEmploymentChecksWithRateLimiterOrchestratorTask";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                // Get the next request
                var employmentCheckCacheRequest = await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null);

                if (employmentCheckCacheRequest != null)
                {
                    // Do the employment status check on this request
                    await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), employmentCheckCacheRequest);
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(GetEmploymentCheckCacheRequestActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    var sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }

                if (!context.IsReplaying)
                    _logger.LogInformation($"{nameof(ProcessEmploymentCheckRequestsOrchestrator)}: Completed.");
            }
            catch (Exception e)
            {
                _logger.LogError($"\n\n{nameof(ProcessEmploymentCheckRequestsOrchestrator)} Exception caught: {e.Message}. {e.StackTrace}");
            }
            finally
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Completed.");

                // execute the orchestrator again with a new context to process the next message
                // Note: The orchestrator may have been unloaded from memory whilst the activity
                // functions were running so this could be a new instance of the orchestrator which
                // will run though the table storage 'event sourcing' state.
                context.ContinueAsNew(null);
            }

        }
    }
}