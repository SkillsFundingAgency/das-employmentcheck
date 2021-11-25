using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    // There are effectively 2 processes (orchestrator durable functions) used to implement this solution, see the diagram below:
    //
    // This code is a top-level orchestrator that calls the orchestrators for Process 1 and process 2 which then run
    // in a loop asynchronously.
    //
    // +------------------------------------+                                     +------------------------------------+
    // |             Process 1              |                                     |              Process 2             |
    // | 1. Get the apprentices requiring   |                                     | 1. Get the next employment check   |
    // | an employment check from the       |                                     | message from the message queue.    |
    // | EmploymentChecks database.         |                                     |                                    |
    // |                                    |         +------------------+        | 2. Call the HMRC API to get the    |
    // | 2. Lookup the related apprentice   | (write) |                  | (read) | employment status for the given    |
    // | National Insurance Numbers and     |-------->| (Database Queue) |------->| apprentice in the employment check |
    // | Employer Paye Schemes (via API's). |         |                  |        | message.                           |
    // |                                    |         +------------------+        |                                    |
    // | 3. Store the enriched apprentice   |                                     | 3. Store result of the employment  |
    // | data in a database message queue   |                                     | check, remove and archive the      |
    // | (to handle the differences in      |                                     | employment check message from      |
    // | processing rates between Process   |                                     | the message queue. Repeat process. |
    // | and Process 2). Repeat process.    |                                     |                                    |
    // +------------------------------------+                                     +------------------------------------+

    /// <summary>
    /// The top-level orchestrator to check the employment status of apprentices.
    /// </summary>
    public class ApprenticeEmploymentChecksOrchestrator
    {
        private const string ThisClassName = "\n\nApprenticeEmploymentChecksOrchestrator";

        private ILogger<ApprenticeEmploymentChecksOrchestrator> _logger;

        /// <summary>
        /// The ApprenticeEmploymentChecksOrchestrator orchestrator constructor, used to initialise the logging component.
        /// </summary>
        /// <param name="logger"></param>
        public ApprenticeEmploymentChecksOrchestrator(
            ILogger<ApprenticeEmploymentChecksOrchestrator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The top-level orchestrator task entry point.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Task</returns>
        [FunctionName(nameof(ApprenticeEmploymentChecksOrchestrator))]
        public async Task ApprenticeEmploymentChecksOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.ApprenticeEmploymentChecksOrchestrator()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

#if DEBUG
                //await context.CallSubOrchestratorAsync(nameof(ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator), 0);
#endif
                // TODO: This 'await' version is just for testing in isolation, delete after test.
                //await context.CallSubOrchestratorAsync(nameof(CreateApprenticeEmploymentChecksOrchestrator), 0);

                // TODO: This 'await' version is just for testing in isolation, delete after test.
                await context.CallSubOrchestratorAsync(nameof(ProcessApprenticeEmploymentChecksOrchestrator), 0);

                //var createApprenticeEmploymentChecksTask = context.CallSubOrchestratorAsync(nameof(CreateApprenticeEmploymentChecksOrchestrator), 0);
                //var processApprenticeEmploymentChecksTask = context.CallSubOrchestratorAsync(nameof(ProcessApprenticeEmploymentChecksOrchestrator), 0);

                //await Task.WhenAll(createApprenticeEmploymentChecksTask, processApprenticeEmploymentChecksTask);

                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}

