using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System.Threading;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    // There are effectively 2 processes (orchestrator durable functions) used to implement this solution, see the diagram below:
    //
    // This is Process 2 in the following diagram.
    //
    /// +------------------------------------+                                     +------------------------------------+
    /// |             Process 1              |                                     |              Process 2             |
    /// | 1. Get the apprentices requiring   |                                     | 1. Get the next employment check   |
    /// | an employment check from the       |                                     | message from the message queue.    |
    /// | EmploymentChecks database.         |                                     |                                    |
    /// |                                    |         +------------------+        | 2. Call the HMRC API to get the    |
    /// | 2. Lookup the related apprentice   | (write) |                  | (read) | employment status for the given    |
    /// | National Insurance Numbers and     |-------->| (Database Queue) |------->| apprentice in the employment check |
    /// | Employer Paye Schemes (via API's). |         |                  |        | message.                           |
    /// |                                    |         +------------------+        |                                    |
    /// | 3. Store the enriched apprentice   |                                     | 3. Store result of the employment  |
    /// | data in a database message queue   |                                     | check, remove and archive the      |
    /// | (to handle the differences in      |                                     | employment check message from      |
    /// | processing rates between Process   |                                     | the message queue. Repeat process. |
    /// | and Process 2). Repeat process.    |                                     |                                    |
    /// +------------------------------------+                                     +------------------------------------+

    /// <summary>
    /// This is the orchestrator that calls the HMRC API to check the employment status of an apprentice.
    /// </summary>
    public class ProcessEmploymentChecksOrchestrator
    {
        private const string ThisClassName = "\n\nProcessEmploymentChecksOrchestrator";

        private ILogger<ProcessEmploymentChecksOrchestrator> _logger;

        /// <summary>
        /// The ProcessApprenticeEmploymentChecksOrchestrator constructor, used to initialise the logging component.
        /// </summary>
        /// <param name="logger"></param>
        public ProcessEmploymentChecksOrchestrator(
            ILogger<ProcessEmploymentChecksOrchestrator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The 'process the apprentices requiring an employment check' orchestrator entry point.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [FunctionName(nameof(ProcessEmploymentChecksOrchestrator))]
        public async Task ProcessEmploymentChecksOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.ProcessEmploymentChecksOrchestratorTask()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                // Get the next message off the message queue
                var employmentCheckMessage = await context.CallActivityAsync<EmploymentCheckMessage>(nameof(DequeueApprenticeEmploymentCheckMessageActivity), null);

                if (employmentCheckMessage.Id != 0)
                {
                    // Do the employment status check on this message
                    var updatedEmploymentCheckMessage =
                        await context.CallActivityAsync<EmploymentCheckMessage>(
                            nameof(CheckApprenticeEmploymentStatusActivity), employmentCheckMessage);

                    // Save the employment status back to the database
                    await context.CallActivityAsync(
                        nameof(SaveApprenticeEmploymentCheckResultActivity),
                        updatedEmploymentCheckMessage);
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(DequeueApprenticeEmploymentCheckMessageActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    DateTime sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }

                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Completed.");

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
    }
}
