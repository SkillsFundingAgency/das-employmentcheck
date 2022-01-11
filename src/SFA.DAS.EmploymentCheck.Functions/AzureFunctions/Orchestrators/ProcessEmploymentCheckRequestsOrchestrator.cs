using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    /// <summary>
    /// This is the orchestrator that calls the HMRC API to check the employment status of an apprentice.
    /// </summary>
    public class ProcessEmploymentCheckRequestsOrchestrator
    {
        private const string ThisClassName = "\n\nProcessEmploymentChecksOrchestrator";
        private readonly ILogger<ProcessEmploymentCheckRequestsOrchestrator> _logger;

        /// <summary>
        /// The ProcessApprenticeEmploymentChecksOrchestrator constructor, used to initialise the logging component.
        /// </summary>
        /// <param name="logger"></param>
        public ProcessEmploymentCheckRequestsOrchestrator(
            ILogger<ProcessEmploymentCheckRequestsOrchestrator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The 'process the apprentices requiring an employment check' orchestrator entry point.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [FunctionName(nameof(ProcessEmploymentCheckRequestsOrchestrator))]
        public async Task ProcessEmploymentCheckCacheRequestsOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.ProcessEmploymentChecksOrchestratorTask";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                // Get the next message off the message queue
                var employmentCheckMessage = await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null);

                if (employmentCheckMessage != null)
                {
                    // Do the employment status check on this message
                    var updatedEmploymentCheckMessage =
                        await context.CallActivityAsync<EmploymentCheckCacheRequest>(
                            nameof(GetHmrcLearnerEmploymentStatusActivity), employmentCheckMessage);

                    // Save the employment status back to the database
                    await context.CallActivityAsync(
                        nameof(StoreEmploymentCheckResultActivity),
                        updatedEmploymentCheckMessage);
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(GetEmploymentCheckCacheRequestActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    var sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
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
