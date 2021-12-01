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
    public class ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator
    {
        private const string ThisClassName = "\n\nApprenticeEmploymentChecksSeedDatabaseSubOrchestrator";
        private ILogger<ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator> _logger;

        public ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator(
            ILogger<ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator))]
        public async Task SeedEmploymentCheckTestDataSubOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.ApprenticeEmploymentChecksSeedDatabaseSubOrchestrator()";

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
