using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator
    {
        #region Private members
        private readonly ILogger<ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator> _logger;

        public ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator(
            ILogger<ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator> logger)
        {
            _logger = logger;
        }
        #endregion Private members

        #region ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator
        [FunctionName(nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator))]
        public async Task ProcessEmploymentChecksWithRateLimiterOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)}.ProcessEmploymentChecksWithRateLimiterOrchestratorTask";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                // Get the next request
                var employmentCheckCacheRequest = await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null);

                if (employmentCheckCacheRequest != null)
                {
                    // Do the employment status check on this request
                    var result = await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), employmentCheckCacheRequest);

                    // Save the employment status back to the database
                    await context.CallActivityAsync(nameof(StoreEmploymentCheckResultActivity), result);

                    // Execute RateLimiter
                    var delayTimeSpan = await context.CallActivityAsync<TimeSpan>(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity), result);

                    // Rate limiter delay between each call
                    var delay = context.CurrentUtcDateTime.Add(delayTimeSpan);
                    await context.CreateTimer(delay, CancellationToken.None);
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(GetEmploymentCheckCacheRequestActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    var sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }

                if (!context.IsReplaying)
                    _logger.LogInformation($"{nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)}: Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)} Exception caught: {ex.Message}. {ex.StackTrace}");
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
    #endregion ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator
}