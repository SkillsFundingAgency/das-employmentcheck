using Ardalis.GuardClauses;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Functions.Activities;
using SFA.DAS.EmploymentCheck.Functions.GetEmploymentCheckCacheRequest;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessEmploymentCheckCacheRequestsOrchestrator
    {
        #region Private members
        private readonly ILogger<ProcessEmploymentCheckCacheRequestsOrchestrator> _logger;

        public ProcessEmploymentCheckCacheRequestsOrchestrator(
            ILogger<ProcessEmploymentCheckCacheRequestsOrchestrator> logger)
        {
            _logger = logger;
        }
        #endregion Private members

        #region ProcessEmploymentChecksCacheRequestsOrchestratorTask
        /// <summary>
        /// Process Employment Check Cache Requests one-at-a-time where the Employed flag is null
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [FunctionName(nameof(ProcessEmploymentCheckCacheRequestsOrchestrator))]
        public async Task ProcessEmploymentChecksCacheRequestsOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{nameof(ProcessEmploymentCheckCacheRequestsOrchestrator)}.ProcessEmploymentChecksCacheRequestsOrchestratorTask()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                // Get the next employment check cache request
                var employmentCheckCachRequest = await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetNextEmploymentCheckCacheRequestActivity), null);

                if (employmentCheckCachRequest != null && employmentCheckCachRequest.Id != 0) // if there was no more employment check data we may have a blank request so skip processing it
                {
                    // Do the employment status check on this request
                    var result = await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetLearnerEmploymentStatusActivity), employmentCheckCachRequest);
                    Guard.Against.Null(result, nameof(result));

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
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(GetNextEmploymentCheckCacheRequestActivity)}  === [NO EMPLOYMENT CHECK REQUESTS FOUND TO PROCESS] === no requests had an Employed value of null and have not previously been processed, sleeping for 10 seconds.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    DateTime sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }

                if (!context.IsReplaying)
                    _logger.LogInformation($"{nameof(ProcessEmploymentCheckCacheRequestsOrchestrator)}: Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{nameof(ProcessEmploymentCheckCacheRequestsOrchestrator)} Exception caught: {ex.Message}. {ex.StackTrace}");
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
    #endregion ProcessEmploymentChecksCacheRequestsOrchestratorTask
}