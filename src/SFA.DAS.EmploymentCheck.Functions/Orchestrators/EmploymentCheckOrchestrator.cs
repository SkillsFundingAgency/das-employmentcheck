using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Orchestrators
{
    public class EmploymentCheckOrchestrator
    {
        private ILogger<EmploymentCheckOrchestrator> _logger;

        public EmploymentCheckOrchestrator(ILogger<EmploymentCheckOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(EmploymentCheckOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            if (!context.IsReplaying)
                _logger.LogInformation("Employment check process started");

            var apprenticesToCheck = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetApprenticesToCheck), null);

            foreach (var result in apprenticesToCheck)
            {
                _logger.LogInformation("ULN: " + result.ULN);
            }

            _logger.LogInformation("Employment check process completed successfully");
        }
    }
}
