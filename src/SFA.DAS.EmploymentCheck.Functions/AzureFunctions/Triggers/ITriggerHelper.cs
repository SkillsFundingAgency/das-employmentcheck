using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public interface ITriggerHelper
    {
        Task<OrchestrationStatusQueryResult> GetRunningInstances(
            string orchestratorName,
            string instanceIdPrefix,
            IDurableOrchestrationClient starter,
            ILogger log);

        Task<HttpResponseMessage> StartTheEmploymentCheckOrchestrators(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            ILogger log,
            ITriggerHelper triggerHelper);

        Task<HttpResponseMessage> StartOrchestrator(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            ILogger log,
            ITriggerHelper triggerHelper,
            string orchestratorName,
            string triggerName);
    }
}
