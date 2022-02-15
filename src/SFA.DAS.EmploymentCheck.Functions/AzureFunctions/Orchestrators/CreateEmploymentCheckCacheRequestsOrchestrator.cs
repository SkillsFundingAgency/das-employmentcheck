using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestsOrchestrator
    {
        private const string ThisClassName = nameof(CreateEmploymentCheckCacheRequestsOrchestrator);
        private readonly ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> _logger;

        public CreateEmploymentCheckCacheRequestsOrchestrator(
            ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsOrchestrator))]
        public async Task CreateEmploymentCheckRequestsTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckRequestsTask";

            // Note: The story says to remove this orchestrator and move its logic to the 'outer' calling orchestrator.
            //       However, the 'outer' orchestrator only runs once so this code would then only run once instead of in a loop.
            //       Discussed with Craig and the decision was to remove the 'outer' calling orchestrator as that just starts
            //       the 'create' and 'process' orchestrators and to refactor the original verson of this 'create' orchestrator
            //       to use a sub orchestrator that does 'one-at-a-time' processing and re-use the existing code by retaining
            //       the batch processing lists but with the lists containing only one employment check.
            //       A subsequent story will handle the refactoring of the related code remove the lists.
            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                var employmentCheckBatch = await context.CallActivityAsync<IList<Application.Models.EmploymentCheck>>(nameof(GetEmploymentChecksBatchActivity), new Object());
                if (employmentCheckBatch != null && employmentCheckBatch.Count > 0)
                {
                    foreach (var employmentCheck in employmentCheckBatch)
                    {
                        // This sub orchestrator is effectively the 'Unit of work' so we need to prevent exceptions
                        // from the 'called' code leaving the foreach loop and skipping rows in the batch.
                        try
                        {
                             await context.CallSubOrchestratorAsync<EmploymentCheckCacheRequest>(nameof(CreateEmploymentCheckCacheRequestOrchestrator), employmentCheck);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: Nothing to process, waiting for 10 seconds before retrying");
                    var sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {e.Message}. {e.StackTrace}");
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
