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
                // We now have all the data we need for the employment check so create a message on the message queue ready for the employment check orchestrator to process
                await context.CallActivityAsync(nameof(CreateRateLimiterOptionsActivity), context.InstanceId);
            }

            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{nameof(CreateRateLimiterOptionsOrchestrator)} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
