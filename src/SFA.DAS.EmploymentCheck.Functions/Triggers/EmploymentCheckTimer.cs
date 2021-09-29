using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Orchestrators;

namespace SFA.DAS.EmploymentCheck.Functions.Triggers
{
    public class EmploymentCheckTimer
    {
        [FunctionName(nameof(EmploymentCheckTimer))]
        public async Task Run([TimerTrigger("%EmploymentCheckTriggerTime%", RunOnStartup = false)]TimerInfo myTimer, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation("Auto Triggering EmploymentCheckOrchestrator");

            string instanceId = await starter.StartNewAsync(nameof(EmploymentCheckOrchestrator));

            log.LogInformation($"Auto Started EmploymentCheckOrchestrator with ID = '{instanceId}'.");
        }
    }
}
