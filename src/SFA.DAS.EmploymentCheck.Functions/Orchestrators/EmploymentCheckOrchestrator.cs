using System;
using System.Collections.Generic;
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

            //var checkTasks = new List<Task>();
            foreach (var apprentice in apprenticesToCheck)
            {
                await context.CallActivityWithRetryAsync(nameof(CheckApprentice), new RetryOptions(new TimeSpan(0, 0, 0, 10), 6),  apprentice);
            }

            //await Task.WhenAll(checkTasks);

            _logger.LogInformation("Employment check process completed successfully");
        }
    }
}
