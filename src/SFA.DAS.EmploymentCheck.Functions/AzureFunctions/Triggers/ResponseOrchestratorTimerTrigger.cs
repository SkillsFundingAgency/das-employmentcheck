using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class ResponseOrchestratorTimerTrigger
    {
        private const string InstanceIdPrefix = "Response-";

        [FunctionName(nameof(ResponseOrchestratorTimerTriggerTask))]
        public static async Task ResponseOrchestratorTimerTriggerTask([TimerTrigger("%ResponseOrchestratorTriggerTime%")]
            TimerInfo timerInfo,
           [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            var triggerHelper = new TriggerHelper();
            var existingInstances = await triggerHelper.GetRunningInstances(nameof(ResponseOrchestratorTimerTrigger),
                InstanceIdPrefix, starter, log);

            if (!existingInstances.DurableOrchestrationState.Any())
            {
                log.LogInformation($"Triggering {nameof(ResponseOrchestrator)}");

                var instanceId = await starter.StartNewAsync(nameof(ResponseOrchestrator), $"{InstanceIdPrefix}{Guid.NewGuid()}");

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            }
        }
    }
}