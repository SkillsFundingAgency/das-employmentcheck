using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateRateLimiterOptionsOrchestrator
    {
        private readonly ILogger<CreateRateLimiterOptionsOrchestrator> _logger;
    
        public CreateRateLimiterOptionsOrchestrator(ILogger<CreateRateLimiterOptionsOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(CreateRateLimiterOptionsOrchestrator))]
        public async Task CreateRateLimiterOptionsOrchestratorSubOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                await context.CallActivityAsync(nameof(CreateRateLimiterOptionsActivity), context.InstanceId);
            }

            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{nameof(CreateRateLimiterOptionsOrchestrator)} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
