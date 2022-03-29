using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class ResetEmploymentChecksMessageSentDateOrchestratorHttpTrigger
    {
        private const string InstancePrefix = "ResetEmploymentCheckMessageSentDate-";

        [FunctionName(nameof(ResetEmploymentChecksMessageSentDateOrchestratorHttpTrigger))]
        public static async Task<HttpResponseMessage> HttpStart(
             [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/ResetEmploymentChecksMessageSentDateOrchestratorHttpTrigger")] HttpRequestMessage httpRequestMessage,
             [DurableClient] IDurableOrchestrationClient starter,
             ILogger log)
        {
            var id = Guid.NewGuid();
            var instanceId = $"{InstancePrefix}{id}";
            var timeout = TimeSpan.FromSeconds(60);
            var retryInterval = TimeSpan.FromSeconds(5);
            var employmentCheckMessageSentData = httpRequestMessage.RequestUri.Query.Split("?")[1].ToLower();

            log.LogInformation($"Triggering {nameof(ResetEmploymentChecksMessageSentDateOrchestrator)}");
            var instance = await starter.StartNewAsync(nameof(ResetEmploymentChecksMessageSentDateOrchestrator), instanceId, employmentCheckMessageSentData);

            return await starter.WaitForCompletionOrCreateCheckStatusResponseAsync(httpRequestMessage, instance, timeout, retryInterval);
        }
    }
}
