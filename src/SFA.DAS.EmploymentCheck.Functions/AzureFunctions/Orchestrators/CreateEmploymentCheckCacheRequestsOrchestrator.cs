using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestsOrchestrator
    {
        #region Private members
        private const string ThisClassName = "\n\nCreateEmploymentCheckRequestsOrchestrator";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> _logger;
        #endregion Private members

        #region Constructors
        /// <summary>
        /// Constructor, used to initialise the logging component.
        /// </summary>
        /// <param name="logger"></param>
        public CreateEmploymentCheckCacheRequestsOrchestrator(
            ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> logger)
        {
            _logger = logger;
        }
        #endregion Constructors

        #region CreateEmploymentCheckRequests
        /// <summary>
        /// The orchestrator entry point.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Task</returns>
        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsOrchestrator))]
        public async Task CreateEmploymentCheckRequestsTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckRequestsTask()"; // Note: Can't use GetCurrentMethod() here as it returns something like 'Move_Next() from the durable task framework

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                // Get a batch of employment checks
                var employmentCheckBatch = await context.CallActivityAsync<IList<Application.Models.EmploymentCheck>>(nameof(GetEmploymentChecksBatchActivity), new Object());

                // Get the Nino's and PAYE schemes for the employment checks batch
                if (employmentCheckBatch.Count > 0)
                {
                    // Get the National Insurance Numbers for each learner in the employment checks batch
                    var getLearnerNiNumbersActivityTask = context.CallActivityAsync<IList<LearnerNiNumber>>(nameof(GetLearnerNiNumbersActivity), employmentCheckBatch);

                    // Get the PAYE scheme(s) for the employment checks batch
                    var employerPayeSchemesTask = context.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), employmentCheckBatch);

                    // Wait for the asynchronous NI number and PAYE scheme(s) API calls to finish
                    await Task.WhenAll(getLearnerNiNumbersActivityTask, employerPayeSchemesTask);

                    // Create an EmploymentCheckCacheRequest for each combination of Nino, Payescheme, MinDate and MaxDate
                    await context.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestsActivity), new EmploymentCheckData(employmentCheckBatch, getLearnerNiNumbersActivityTask.Result, employerPayeSchemesTask.Result));
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: No data found, sleeping for 10 seconds before executing the orchestrator again");

                    // TODO: Logic for re-executing failed requests in the 'sleep' time when there are no other requests to process

                    // No data found so sleep for 10 seconds then execute the orchestrator again
                    DateTime sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
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
        #endregion CreateEmploymentCheckRequests
    }
}
