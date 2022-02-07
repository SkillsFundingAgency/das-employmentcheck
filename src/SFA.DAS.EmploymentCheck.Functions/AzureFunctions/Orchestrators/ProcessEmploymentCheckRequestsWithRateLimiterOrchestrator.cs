using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator
    {
        private readonly ILogger<ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator> _logger;

        public ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator(
            ILogger<ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator> logger)
        {
            _logger = logger;
        }

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

                    // Set the RequestCompletionStatus for both the EmploymentCheck and the EmploymentCheckCacheRequest to 'Completed'
                    await context.CallActivityAsync(nameof(StoreEmploymentCheckResultActivity), result);

                    // If an employment check has confirmed that the apprentice is employed on the given PAYE scheme
                    // then we don't need to process any remaining employment check requests for that apprentice
                    // and can set the RequestCompletionStatus for any remaining checks for this apprentice to 'Skipped'
                    if (result != null && result.Employed.Value)
                    {
                        var relatedEmploymentCheckCacheResults = await context.CallActivityAsync<IList<EmploymentCheckCacheRequest>>(nameof(SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusActivity), new Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>(result, ProcessingCompletionStatus.Skipped));
                        CheckRelateRequestsRequestCompletionStatusIsSet(thisMethodName, relatedEmploymentCheckCacheResults);
                    }

                    // Execute RateLimiter
                    var delayTimeSpan = await context.CallActivityAsync<TimeSpan>(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity), result);

                    // Rate limiter delay between each call
                    var delay = context.CurrentUtcDateTime.Add(delayTimeSpan);
                    await context.CreateTimer(delay, CancellationToken.None);
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {nameof(GetEmploymentCheckCacheRequestActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    var sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }

                if (!context.IsReplaying)
                    _logger.LogInformation($"{nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)}: Completed.");
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)} Exception caught: {e.Message}. {e.StackTrace}");
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

        private void CheckRelateRequestsRequestCompletionStatusIsSet(string thisMethodName, IList<EmploymentCheckCacheRequest> relatedEmploymentCheckCacheResults)
        {
            if (relatedEmploymentCheckCacheResults != null && relatedEmploymentCheckCacheResults.Count > 0)
            {
                foreach (var request in relatedEmploymentCheckCacheResults)
                {
                    if (request != null && request.RequestCompletionStatus.Value != (short)ProcessingCompletionStatus.Skipped)
                    {
                        _logger.LogError($"{thisMethodName} ERROR: Failed to set the request completion status to 'Skipped' for EmploymentCheckCacheRequest Id = [{request.Id}]");
                    }
                }
            }
        }
    }
}