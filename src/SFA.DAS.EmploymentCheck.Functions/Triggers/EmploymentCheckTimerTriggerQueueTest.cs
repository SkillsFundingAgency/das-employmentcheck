using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Functions.Triggers
{
    public class EmploymentCheckTimerTriggerQueueTest
    {
        [FunctionName(nameof(EmploymentCheckTimerTriggerQueueTest))]
        public async Task Run([TimerTrigger("0/1 * * * * *", RunOnStartup = true)]TimerInfo myTimer, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation($"[{DateTime.Now}] Triggering EmploymentCheckOrchestrator");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}
