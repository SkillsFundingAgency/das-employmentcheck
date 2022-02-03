using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public interface ITriggerHelper
    {
        public Task<OrchestrationStatusQueryResult> GetRunningInstances(string orchestratorName, string instanceIdPrefix, IDurableOrchestrationClient starter, ILogger log);
    }
}