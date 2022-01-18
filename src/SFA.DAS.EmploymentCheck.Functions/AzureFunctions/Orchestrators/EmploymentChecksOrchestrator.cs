using System;
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
        private readonly ILogger<EmploymentChecksOrchestrator> _logger;
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
            var thisMethodName = $"{ThisClassName}.EmploymentChecksOrchestratorTask";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                var getEmploymentChecksTask = context.CallSubOrchestratorAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), 0);
                var processEmploymentChecksTask = context.CallSubOrchestratorAsync(nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator), 0);

                await Task.WhenAll(getEmploymentChecksTask, processEmploymentChecksTask);

                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
        #endregion EmploymentChecksOrchestrator
    }
}