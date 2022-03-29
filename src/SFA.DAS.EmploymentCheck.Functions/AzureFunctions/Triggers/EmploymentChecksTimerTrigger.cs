using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Extensions;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class EmploymentChecksTimerTrigger
    {
        [FunctionName(nameof(EmploymentChecksTimerTrigger))]
        public static async Task EmploymentChecksTimerTriggerTask(
            [TimerTrigger("%EmploymentChecksTimerTriggerTime%")] TimerInfo timerInfo,
            [DurableClient] IDurableOrchestrationClient starter, ILogger log
        )
        {
            await starter.StartIfNotRunning(nameof(CreateEmploymentCheckCacheRequestsOrchestrator));
            await starter.StartIfNotRunning(nameof(ProcessEmploymentCheckRequestsOrchestrator));
        }
    }
}
