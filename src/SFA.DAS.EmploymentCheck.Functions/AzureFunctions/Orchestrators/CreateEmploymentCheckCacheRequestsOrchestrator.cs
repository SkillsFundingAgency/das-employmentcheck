using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestsOrchestrator
    {
        private const string ThisClassName = "\n\nCreateEmploymentCheckRequestsOrchestrator";

        private readonly ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> _logger;

        /// <summary>
        /// Constructor, used to initialise the logging component.
        /// </summary>
        /// <param name="logger"></param>
        public CreateEmploymentCheckCacheRequestsOrchestrator(
            ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// The orchestrator entry point.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Task</returns>
        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsOrchestrator))]
        public async Task CreateEmploymentCheckRequestsTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckRequestsTask"; // Note: Can't use GetCurrentMethod() here as it returns something like 'Move_Next() from the durable task framework

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                var employmentCheckBatch = await context.CallActivityAsync<IList<Application.Models.EmploymentCheck>>(nameof(GetEmploymentChecksBatchActivity), new Object());
                if (employmentCheckBatch.Count > 0)
                {
                    var dbLearnerNiNumbers = await context.CallActivityAsync<IList<LearnerNiNumber>>(nameof(GetDbLearnerNiNumbersActivity), employmentCheckBatch);
                    var employmentChecksWithoutDbNiNumbers = employmentCheckBatch.Where(ec => !dbLearnerNiNumbers.Any(ni => ni.Uln == ec.Uln)).ToList();

                    if(employmentChecksWithoutDbNiNumbers == null)
                    {
                        employmentChecksWithoutDbNiNumbers = new List<Application.Models.EmploymentCheck>();
                    }

                    var getLearnerNiNumbersActivityTask = context.CallActivityAsync<IList<LearnerNiNumber>>(nameof(GetLearnerNiNumbersActivity), employmentChecksWithoutDbNiNumbers);

                    var employerPayeSchemesTask = context.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), employmentCheckBatch);
                    await Task.WhenAll(getLearnerNiNumbersActivityTask, employerPayeSchemesTask);

                    var learnerNiNumbers = getLearnerNiNumbersActivityTask.Result;
                    foreach (var dbLearnerNiNumber in dbLearnerNiNumbers)
                    {
                        if (!learnerNiNumbers.Contains(dbLearnerNiNumber))
                            learnerNiNumbers.Add(dbLearnerNiNumber);
                    }

                    var employerPayeSchemes = employerPayeSchemesTask.Result;
                    await context.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestsActivity), new EmploymentCheckData(employmentCheckBatch, learnerNiNumbers, employerPayeSchemes));
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: Nothing to process, waiting for 10 seconds before retrying");

                    // TODO: Logic for re-executing failed requests in the 'sleep' time when there are no other requests to process

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