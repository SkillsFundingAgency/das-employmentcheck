using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class EmploymentChecksHttpTrigger
    {
        [FunctionName("EmploymentChecksHttpTrigger")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/EmploymentChecksOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log
        )
        {
            ITriggerHelper triggerHelper = new TriggerHelper();
            var createRequestsOrchestratorName = nameof(CreateEmploymentCheckCacheRequestsOrchestrator);
            var createRequestsOrchestratorTriggerName = nameof(CreateEmploymentCheckRequestsOrchestratorTrigger);
            var createRequestsOrchestratorInstancePrefix = $"CreateEmploymentCheck-{Guid.NewGuid()}";
            var procesRequestsOrchestratorName = nameof(ProcessEmploymentCheckRequestsOrchestrator);
            var processRequestsOrchestratorTriggerName = nameof(ProcessEmploymentChecksHttpTrigger);
            var processRequestsOrchestratorInstancePrefix = $"ProcessEmploymentCheck-{Guid.NewGuid()}";

            return await triggerHelper.StartTheEmploymentCheckOrchestrators(
                req,
                starter,
                log,
                triggerHelper,
                createRequestsOrchestratorName,
                createRequestsOrchestratorTriggerName,
                createRequestsOrchestratorInstancePrefix,
                procesRequestsOrchestratorName,
                processRequestsOrchestratorTriggerName,
                processRequestsOrchestratorInstancePrefix);
        }
    }
}
