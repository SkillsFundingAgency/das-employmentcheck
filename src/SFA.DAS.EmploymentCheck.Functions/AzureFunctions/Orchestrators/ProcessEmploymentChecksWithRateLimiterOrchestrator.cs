using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessEmploymentChecksWithRateLimiterOrchestrator
    {
        private readonly ILogger<ProcessEmploymentChecksWithRateLimiterOrchestrator> _logger;

        public ProcessEmploymentChecksWithRateLimiterOrchestrator(
            ILogger<ProcessEmploymentChecksWithRateLimiterOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(ProcessEmploymentChecksWithRateLimiterOrchestrator))]
        public async Task ProcessEmploymentChecksWithRateLimiterOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{nameof(ProcessEmploymentChecksWithRateLimiterOrchestrator)}.ProcessEmploymentChecksWithRateLimiterOrchestratorTask()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                // Get the next message off the message queue
                var employmentCheckMessage = await context.CallActivityAsync<EmploymentCheckMessage>(nameof(DequeueApprenticeEmploymentCheckMessageActivity), null);

                if (employmentCheckMessage.Id != 0)
                {
                    // Do the employment status check on this message
                    var result = await context.CallActivityAsync<EmploymentCheckMessage>(
                        nameof(CheckApprenticeEmploymentStatusActivity), employmentCheckMessage);


                    // Save the employment status back to the database
                    await context.CallActivityAsync(nameof(SaveApprenticeEmploymentCheckResultActivity), result);

                    // Execute RateLimiter
                    var delayTimeSpan = await context.CallActivityAsync<TimeSpan>(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity), result);

                    // Rate limiter delay between each call
                    var delay = context.CurrentUtcDateTime.Add(delayTimeSpan);
                    await context.CreateTimer(delay, CancellationToken.None);
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(DequeueApprenticeEmploymentCheckMessageActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    DateTime sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);

                }

                if (!context.IsReplaying)
                    _logger.LogInformation($"{nameof(ProcessEmploymentChecksWithRateLimiterOrchestrator)}: Completed.");

                // execute the orchestrator again with a new context to process the next message
                // Note: The orchestrator may have been unloaded from memory whilst the activity
                // functions were running so this could be a new instance of the orchestrator which
                // will run though the table storage 'event sourcing' state.
                context.ContinueAsNew(null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{nameof(ProcessEmploymentChecksWithRateLimiterOrchestrator)} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }

    }
}