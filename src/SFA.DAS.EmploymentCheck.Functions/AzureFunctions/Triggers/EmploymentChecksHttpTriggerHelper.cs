using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public class EmploymentChecksHttpTriggerHelper
        : IEmploymentChecksHttpTriggerHelper
    {
        public EmploymentChecksHttpTriggerHelper() { }

        public HttpRequestMessage Req { get; set; }

        public IDurableOrchestrationClient Starter { get; set; }

        public  ILogger Log { get; set; }

        public async Task<Tuple<string, HttpResponseMessage>> StartCreateRequestsOrchestrator(
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

        public async Task<Tuple<string, HttpResponseMessage>> StartProcessRequestsOrchestrator(
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

        public async Task<string> FormatResponseString(
            Tuple<string, HttpResponseMessage> httpResponseMessage,
            string orchestratorName)
        {
            var resultContentString = httpResponseMessage.Item2.ToString();
            var stringContent = $"Started Orchestrator[{orchestratorName}] Id[{httpResponseMessage.Item1}] : ";
            resultContentString = stringContent + resultContentString;

            var cfirstComma = resultContentString.IndexOf(",");
            resultContentString = resultContentString.Substring(0, cfirstComma + 26); // trim the string after the 'Accepted' status code

            return await Task.FromResult(resultContentString);
        }

        public async Task<HttpResponseMessage> CreateHttpResponseMessage(
            string createResultContentString,
            string processResultContentString
        )
        {
            HttpResponseMessage resultMessage = new HttpResponseMessage(HttpStatusCode.Accepted);
            resultMessage.Content = new StringContent($"{createResultContentString}\n{processResultContentString}\n");

            return await Task.FromResult(resultMessage);
        }
    }
}
