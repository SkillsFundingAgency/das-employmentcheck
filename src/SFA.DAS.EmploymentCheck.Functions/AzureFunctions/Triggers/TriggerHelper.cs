using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public class TriggerHelper
    {
        public async Task<OrchestrationStatusQueryResult> GetRunningInstances(string orchestratorName, string instanceIdPrefix, IDurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation($"Checking for running instances of {orchestratorName}");

            var runningInstances = await starter.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = instanceIdPrefix,
                RuntimeStatus = new[]
                {
                    OrchestrationRuntimeStatus.Pending,
                    OrchestrationRuntimeStatus.Running,
                    OrchestrationRuntimeStatus.ContinuedAsNew
                }
            }, System.Threading.CancellationToken.None);

            return runningInstances;
        }
    }
}