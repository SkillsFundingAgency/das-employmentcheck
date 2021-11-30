using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator
    {
        private readonly ILogger<ProcessApprenticeEmploymentChecksOrchestrator> _logger;

        public ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator(
            ILogger<ProcessApprenticeEmploymentChecksOrchestrator> logger)
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
                   
                    return;
                }

                // Do the employment status check on this message
                var result = await context.CallActivityAsync<ApprenticeEmploymentCheckMessageModel>(
                    nameof(CheckApprenticeEmploymentStatusActivity), apprenticeEmploymentCheckMessage);


                // Save the employment status back to the database
                await context.CallActivityAsync(nameof(SaveApprenticeEmploymentCheckResultActivity), result);

                // Execute RateLimiter
                var delayTimeSpan = await context.CallActivityAsync<TimeSpan>(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity), result);

                // Rate limiter delay between each call
                var delay = context.CurrentUtcDateTime.Add(delayTimeSpan);
                await context.CreateTimer(delay, CancellationToken.None);


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
                _logger.LogError($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }

        private static async Task<IList<EmploymentCheckMessage>> GetNextMessagesOffTheQueue(IDurableOrchestrationContext context,
            HmrcApiRateLimiterOptions options)
        {
            var requests = new List<EmploymentCheckMessage>();

            for (var i = 0; i < options.BatchSize; i++)
            {
                requests.Add(
                    await context.CallActivityAsync<EmploymentCheckMessage>(
                        nameof(DequeueApprenticeEmploymentCheckMessageActivity), null)
                );
            }

            return requests;
        }

        private static async Task<IList<EmploymentCheckMessage>> DoEmploymentStatusChecks(IDurableOrchestrationContext context,
            IEnumerable<EmploymentCheckMessage> apprenticeEmploymentCheckMessages)
        {
            var results = new List<EmploymentCheckMessage>();

            foreach (var message in apprenticeEmploymentCheckMessages)
            {
                results.Add(
                    await context.CallActivityAsync<EmploymentCheckMessage>(
                        nameof(CheckApprenticeEmploymentStatusActivity), message));
            }

            return results;
        }

        private void AdjustRequestDelay(IEnumerable<EmploymentCheckMessage> results, HmrcApiRateLimiterOptions options)
        {
            var tooManyRequests = results.Any(r => string.Equals(r.ResponseId.ToString(), HttpStatusCode.TooManyRequests.ToString(),
                StringComparison.InvariantCultureIgnoreCase));

            if (tooManyRequests)
            {
                options.DelayInMs += options.DelayAdjustmentIntervalInMs;
            }
            else if (options.DelayInMs > options.DelayAdjustmentIntervalInMs)
            {
                options.DelayInMs -= options.DelayAdjustmentIntervalInMs;
            }

            _optionsRepository.UpdateRequestDelaySetting(options.DelayInMs);
        }

        private static async Task SaveResults(IDurableOrchestrationContext context, IEnumerable<EmploymentCheckMessage> results)
        {
            foreach (var result in results)
            {
                await context.CallActivityAsync(nameof(SaveApprenticeEmploymentCheckResultActivity), result);
            }
        }
    }
}
