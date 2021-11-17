using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ApprenticeEmploymentChecksOrchestrator
    {
        private const string ThisClassName = "\n\nApprenticeEmploymentChecksOrchestrator";

        private ILogger<ApprenticeEmploymentChecksOrchestrator> _logger;

        /// <summary>
        /// The top-level orchestrator to check the employment status of apprentices
        /// </summary>
        /// <param name="logger"></param>
        public ApprenticeEmploymentChecksOrchestrator(
            ILogger<ApprenticeEmploymentChecksOrchestrator> logger)
        {
            _logger = logger;
        }

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

                await context.CallSubOrchestratorAsync(nameof(CreateApprenticeEmploymentChecksOrchestrator), 0);

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

