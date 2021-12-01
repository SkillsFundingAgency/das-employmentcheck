using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class SeedEmploymentCheckTestDataOrchestrator
    {
        private const string ThisClassName = "\n\nSeedEmploymentCheckTestDataOrchestrator";
        private ILogger<SeedEmploymentCheckTestDataOrchestrator> _logger;

        public SeedEmploymentCheckTestDataOrchestrator(
            ILogger<SeedEmploymentCheckTestDataOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(SeedEmploymentCheckTestDataOrchestrator))]
        public async Task SeedEmploymentCheckTestDataOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.SeedEmploymentCheckTestDataOrchestratorTask()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Orchestrator Started.");

                await context.CallActivityAsync(nameof(SeedEmploymentCheckTestDataActivity), 0);

                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Orchestrator Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }

        }
    }
}
