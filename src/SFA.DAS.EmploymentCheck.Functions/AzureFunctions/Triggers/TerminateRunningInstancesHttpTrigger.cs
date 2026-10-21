using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Extensions;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class TerminateRunningInstancesHttpTrigger
    {
        [FunctionName(nameof(TerminateRunningInstancesHttpTrigger))]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/TerminateRunningInstancesHttpTrigger")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log
        )
        {
            var runningInstances = await starter.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                RuntimeStatus = DurableOrchestrationClientExtensions.RuntimeStatuses
            }, System.Threading.CancellationToken.None);

            foreach (var orchestrator in runningInstances.DurableOrchestrationState)
            {
                log.LogInformation("TerminateRunningInstancesHttpTrigger: terminating orchestrator instance {InstanceId}",
                    orchestrator.InstanceId);
                await starter.TerminateAsync(orchestrator.InstanceId, "Post-deployment terminate job");
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"Terminated {runningInstances.DurableOrchestrationState.Count()} orchestrator instances.")};
        }
    }
}
