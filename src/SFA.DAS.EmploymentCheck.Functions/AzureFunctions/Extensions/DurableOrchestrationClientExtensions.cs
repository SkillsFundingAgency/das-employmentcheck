using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Extensions
{
    public static class DurableOrchestrationClientExtensions
    {
        public static IEnumerable<OrchestrationRuntimeStatus> RuntimeStatuses => new[]
        {
            OrchestrationRuntimeStatus.Pending,
            OrchestrationRuntimeStatus.Running,
            OrchestrationRuntimeStatus.ContinuedAsNew
        };

        public static async Task StartIfNotRunning(this IDurableOrchestrationClient starter, string orchestrator)
        {
            var runningInstances = await starter.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = orchestrator,
                RuntimeStatus = RuntimeStatuses
            }, System.Threading.CancellationToken.None);

            if (runningInstances.DurableOrchestrationState.Any()) return;

            await starter.StartNewAsync(orchestrator, $"{orchestrator}{Guid.NewGuid()}");
        }
    }
}