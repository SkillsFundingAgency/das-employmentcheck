using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator
    {
        private readonly ILogger<ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator> _logger;

        public ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator(
            ILogger<ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator))]
        public async Task ProcessApprenticeEmploymentChecksSubOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator)}.ProcessApprenticeEmploymentChecksSubOrchestratorTask()";

            try
            { 
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");


                // Get the next message off the message queue
                var apprenticeEmploymentCheckMessage = await context.CallActivityAsync<ApprenticeEmploymentCheckMessageModel>(nameof(DequeueApprenticeEmploymentCheckMessageActivity), null);

                if (apprenticeEmploymentCheckMessage == null)
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(DequeueApprenticeEmploymentCheckMessageActivity)} returned no results. Nothing to process.");
                    context.ContinueAsNew(null);
                }

                // Do the employment status check on this message
                var result = await context.CallActivityAsync<ApprenticeEmploymentCheckMessageModel>(nameof(CheckApprenticeEmploymentStatusActivity), apprenticeEmploymentCheckMessage);

                if (result == null || result.EmploymentCheckId == 0)
                {

                    _logger.LogError($"{nameof(CheckApprenticeEmploymentStatusActivity)} returned null result");
                    
                    context.ContinueAsNew(null);
                }

                if (result != null)
                {

                    // Save the employment status back to the database
                    await context.CallActivityAsync(nameof(SaveApprenticeEmploymentCheckResultActivity), result);

                    // Execute RateLimiter
                    var delayTimeSpan = await context.CallActivityAsync<TimeSpan>(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity), result);

                    // Rate limiter delay between each call
                    var delay = context.CurrentUtcDateTime.Add(delayTimeSpan);
                    await context.CreateTimer(delay, CancellationToken.None);
                }

                if (!context.IsReplaying)
                    _logger.LogInformation($"{nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator)}: Completed.");

                // execute the orchestrator again with a new context to process the next message
                // Note: The orchestrator may have been unloaded from memory whilst the activity
                // functions were running so this could be a new instance of the orchestrator which
                // will run though the table storage 'event sourcing' state.
                context.ContinueAsNew(null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator)} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }

    }
}
