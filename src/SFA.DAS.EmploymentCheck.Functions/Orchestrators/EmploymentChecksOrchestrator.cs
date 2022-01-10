﻿using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    /// <summary>
    /// The top-level orchestrator to check the employment status of apprentices.
    /// </summary>
    public class EmploymentChecksOrchestrator
    {
        #region Private members
        private const string ThisClassName = "\n\nEmploymentChecksOrchestrator";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        private ILogger<EmploymentChecksOrchestrator> _logger;
        #endregion Private members

        #region Constructors
        /// <summary>
        /// The EmploymentChecksOrchestrator constructor, used to initialise the logging component.
        /// </summary>
        /// <param name="logger"></param>
        public EmploymentChecksOrchestrator(
            ILogger<EmploymentChecksOrchestrator> logger)
        {
            _logger = logger;
        }
        #endregion Constructors

        #region EmploymentChecksOrchestrator
        /// <summary>
        /// The top-level orchestrator task entry point.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Task</returns>
        [FunctionName(nameof(EmploymentChecksOrchestrator))]
        public async Task EmploymentChecksOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.EmploymentChecksOrchestrator()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                // TODO: This 'await' version is just for testing in isolation, delete after test.
                //await context.CallSubOrchestratorAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), null);

                // TODO: This 'await' version is just for testing in isolation, delete after test.
                //await context.CallSubOrchestratorAsync(nameof(ProcessEmploymentCheckCacheRequestsOrchestrator), 0);

                var getEmploymentChecksTask = context.CallSubOrchestratorAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), 0);
                var processEmploymentChecksTask = context.CallSubOrchestratorAsync(nameof(ProcessEmploymentCheckCacheRequestsOrchestrator), 0);

                await Task.WhenAll(getEmploymentChecksTask, processEmploymentChecksTask);

                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
        #endregion EmploymentChecksOrchestrator
    }
}