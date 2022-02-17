using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class EmploymentChecksHttpTrigger
    {
        [FunctionName("EmploymentChecksHttpTrigger")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/EmploymentChecksOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var createResult = await StartCreateRequestsOrchestrator(req, starter, log);
            if(!createResult.Item2.IsSuccessStatusCode) { return createResult.Item2; }

            var processResult = await StartProcessRequestsOrchestrator(req, starter, log);
            if (!processResult.Item2.IsSuccessStatusCode) { return processResult.Item2; }

            var createResultContentString = createResult.Item2.ToString();
            var createStringContent = $"Started Orchestrator[{nameof(CreateEmploymentCheckCacheRequestsOrchestrator)}] Id[{createResult.Item1}] : ";
            createResultContentString = createStringContent + createResultContentString;

            var cfirstComma = createResultContentString.IndexOf(",");
            createResultContentString = createResultContentString.Substring(0, cfirstComma + 26); // trim the string after the 'Accepted' status code

            var processResultContentString = processResult.Item2.ToString();
            var processStringContent = $"Started Orchestrator[{nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)}] Id[{processResult.Item1}] : ";
            processResultContentString = processStringContent + processResultContentString;

            var pfirstComma = processResultContentString.IndexOf(",");
            processResultContentString = processResultContentString.Substring(0, pfirstComma + 26);

            HttpResponseMessage resultMessage = new HttpResponseMessage(HttpStatusCode.Accepted);
            resultMessage.Content = new StringContent($"{createResultContentString}\n{processResultContentString}\n");
            return resultMessage;
        }

        private static async Task<Tuple<string, HttpResponseMessage>> StartCreateRequestsOrchestrator(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            ILogger log
        )
        {
            const string InstanceIdPrefix = "CreateEmploymentCheck-";

            var triggerHelper = new TriggerHelper();
            var existingInstances =
                await triggerHelper.GetRunningInstances(nameof(CreateEmploymentCheckRequestsOrchestratorTrigger), InstanceIdPrefix, starter, log);

            if (!existingInstances.DurableOrchestrationState.Any())
            {
                log.LogInformation($"Triggering {nameof(CreateEmploymentCheckCacheRequestsOrchestrator)}");

                var instanceId = await starter.StartNewAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), $"{InstanceIdPrefix}{Guid.NewGuid()}");

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
                var message = starter.CreateCheckStatusResponse(req, instanceId);
                return new Tuple<string, HttpResponseMessage>(instanceId, message);
            }

            var resultMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent($"An instance of {nameof(CreateEmploymentCheckCacheRequestsOrchestrator)} is already running."),
            };

            return new Tuple<string, HttpResponseMessage>(string.Empty, resultMessage);
        }

        private static async Task<Tuple<string, HttpResponseMessage>> StartProcessRequestsOrchestrator(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            ILogger log
        )
        {
            const string InstanceIdPrefix = "ProcessEmploymentCheck-";

            var triggerHelper = new TriggerHelper();
            var existingInstances =
                await triggerHelper.GetRunningInstances(nameof(ProcessEmploymentChecksHttpTrigger), InstanceIdPrefix, starter, log);

            if (!existingInstances.DurableOrchestrationState.Any())
            {
                log.LogInformation($"Triggering {nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)}");

                string instanceId = await starter.StartNewAsync(nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator), $"{InstanceIdPrefix}{Guid.NewGuid()}");

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
                var message = starter.CreateCheckStatusResponse(req, instanceId);
                return new Tuple<string, HttpResponseMessage>(instanceId, message);
            }

            var resultMessage = new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent($"An instance of {nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)} is already running."),
            };

            return new Tuple<string, HttpResponseMessage>(string.Empty, resultMessage);
        }
    }
}
