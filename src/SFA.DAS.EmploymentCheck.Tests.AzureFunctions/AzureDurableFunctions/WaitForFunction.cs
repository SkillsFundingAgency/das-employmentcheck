using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.EmploymentCheck.Tests.AzureFunctions.AzureDurableFunctions
{
    public static class WaitForFunction
    {
        [FunctionName(nameof(WaitForFunction))]
        [NoAutomaticTrigger]
        public static async Task Run([DurableClient] IDurableOrchestrationClient client, string name, TimeSpan? timeout, string expectedCustomStatus)
        {
            using var cts = new CancellationTokenSource();
            if (timeout != null)
            {
                cts.CancelAfter(timeout.Value);
            }

            await client.Wait(status => status.All(x => OrchestrationsCompleteOrAwaitingInput(name, expectedCustomStatus, x)), cts.Token);
        }

        private static bool OrchestrationsCompleteOrAwaitingInput(string orchestratorName, string expectedCustomStatus, DurableOrchestrationStatus orchestrationStatus)
        {
            var customStatus = orchestrationStatus.CustomStatus.ToObject<string>();
            return orchestrationStatus.Name != orchestratorName || (expectedCustomStatus != null && customStatus == expectedCustomStatus);
        }
    }
}
