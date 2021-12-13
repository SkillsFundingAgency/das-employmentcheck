using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    // There are effectively 2 processes (orchestrator durable functions) used to implement this solution, see the diagram below:
    //
    // This is Process 1 in the following diagram.
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
    /// The orchestrator that gets the apprentices that require an employment check.
    /// </summary>
    public class CreateApprenticeEmploymentChecksOrchestrator
    {
        private const string ThisClassName = "\n\nCreateApprenticeEmploymentChecksOrchestrator";

        private ILogger<CreateApprenticeEmploymentChecksOrchestrator> _logger;

        /// <summary>
        /// The CreateApprenticeEmploymentChecksOrchestrator constructor, used to initialise the logging component.
        /// </summary>
        /// <param name="logger"></param>
        public CreateApprenticeEmploymentChecksOrchestrator(
            ILogger<CreateApprenticeEmploymentChecksOrchestrator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The 'create the apprentices requiring an employment check' orchestrator entry point.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Task</returns>
        [FunctionName(nameof(CreateApprenticeEmploymentChecksOrchestrator))]
        public async Task CreateApprenticeEmploymentChecksSubOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateApprenticeEmploymentChecksSubOrchestratorTask()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                // Get the apprentices requiring an employment check (we have to await this call as we can't do anything else until we have the list of apprentices)
                var apprenticeEmploymentChecks =
                    await context.CallActivityAsync<IList<ApprenticeEmploymentCheckModel>>(
                        nameof(GetApprenticeEmploymentChecksActivity), 0);
                _logger.LogInformation(
                    $"\n\n{nameof(GetApprenticeEmploymentChecksActivity)} activity returned {apprenticeEmploymentChecks.Count} results");

                // If we got a batch of apprentices then lookup the Nino and Paye Schemes otherwise sleep for a while before repeating the execution
                if (apprenticeEmploymentChecks.Count > 0)
                {
                    // Get the apprentices National Insurance Numbers
                    var getNationalInsuranceNumbersTask =
                        context.CallActivityAsync<IList<ApprenticeNiNumber>>(nameof(GetApprenticesNiNumberActivity),
                            apprenticeEmploymentChecks);

                    // Get the apprentices employer PAYE schemes
                    var getPayeSchemesTask =
                        context.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployersPayeSchemesActivity),
                            apprenticeEmploymentChecks);

                    // Wait for the NI numbers and PAYE schemes calls to finish before proceeding to add the data to the db message queue
                    await Task.WhenAll(getNationalInsuranceNumbersTask, getPayeSchemesTask);

                    // We now have all the data we need for the employment check so create a message on the message queue ready for the employment check orchestrator to process
                    await context.CallActivityAsync<int>(nameof(EnqueueApprenticeEmploymentCheckMessagesActivity),
                        new ApprenticeRelatedData(apprenticeEmploymentChecks, getNationalInsuranceNumbersTask.Result,
                            getPayeSchemesTask.Result));
                }
                else
                {
                    _logger.LogInformation(
                        $"{thisMethodName}: No data found so sleep for 10 seconds then execute the orchestrator again");
                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    DateTime sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }

            finally
            {
                // execute the orchestrator again with a new context to process the next message
                // Note: The orchestrator may have been unloaded from memory whilst the activity
                // functions were running so this could be a new instance of the orchestrator which
                // will run though the table storage 'event sourcing' state.
                context.ContinueAsNew(null);

                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Completed.");
            }
        }
    }
}
