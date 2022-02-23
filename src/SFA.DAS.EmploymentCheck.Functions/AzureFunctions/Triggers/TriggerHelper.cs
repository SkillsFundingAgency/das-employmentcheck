﻿using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public class TriggerHelper : ITriggerHelper
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

        public async Task<HttpResponseMessage> StartTheEmploymentCheckOrchestrators(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            ILogger log,
            ITriggerHelper triggerHelper,
            string createRequestsOrchestratorName,
            string createRequestsOrchestratorTriggerName,
            string createRequestsOrchestratorInstancePrefix,
            string procesRequestsOrchestratorName,
            string processRequestsOrchestratorTriggerName,
            string processRequestsOrchestratorInstancePrefix
        )
        {
            HttpResponseMessage createRequestsOrchestratorResponseMessage =
                await triggerHelper.StartOrchestrator(
                    req,
                    starter,
                    log,
                    triggerHelper,
                    createRequestsOrchestratorName,
                    createRequestsOrchestratorTriggerName,
                    createRequestsOrchestratorInstancePrefix);
            if (createRequestsOrchestratorResponseMessage.StatusCode != HttpStatusCode.Accepted)
            {
                var content = await createRequestsOrchestratorResponseMessage.Content.ReadAsStringAsync();
                content = $"Unable to start the CreateRequests Orchestrator: {content}\n";
                createRequestsOrchestratorResponseMessage.Content = new StringContent(content);
                return createRequestsOrchestratorResponseMessage;
            }

            HttpResponseMessage processRequestsOrchestratorResponseMessage =
                await triggerHelper.StartOrchestrator(
                    req,
                    starter,
                    log,
                    triggerHelper,
                    procesRequestsOrchestratorName,
                    processRequestsOrchestratorTriggerName,
                    processRequestsOrchestratorInstancePrefix);
            if (processRequestsOrchestratorResponseMessage.StatusCode != HttpStatusCode.Accepted)
            {
                var content = await processRequestsOrchestratorResponseMessage.Content.ReadAsStringAsync();
                content = $"Unable to start the ProcessRequests Orchestrator: [{content}]\n";
                processRequestsOrchestratorResponseMessage.Content = new StringContent(content);
                return processRequestsOrchestratorResponseMessage;
            }

            var browserHttpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent($"{await createRequestsOrchestratorResponseMessage.Content.ReadAsStringAsync()}\n{await processRequestsOrchestratorResponseMessage.Content.ReadAsStringAsync()}")
            };
            return browserHttpResponseMessage;
        }

        public async Task<HttpResponseMessage> StartOrchestrator(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            ILogger log,
            ITriggerHelper triggerHelper,
            string orchestratorName,
            string triggerName,
            string instancePrefix
        )
        {
            var existingInstances =
                await triggerHelper.GetRunningInstances(triggerName, instancePrefix, starter, log);

            if (existingInstances != null && existingInstances.DurableOrchestrationState.Any())
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent($"An instance of {orchestratorName} is already running.") };
                log.LogInformation(await responseMessage.Content.ReadAsStringAsync());
                return responseMessage;
            }

            log.LogInformation($"Triggering {orchestratorName}");
            var instanceId = await starter.StartNewAsync(orchestratorName, instancePrefix);
            if(string.IsNullOrEmpty(instanceId))
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent($"An error occured starting [{orchestratorName}], no instance id was returned.") };
                log.LogInformation(await responseMessage.Content.ReadAsStringAsync());
                return responseMessage;
            }

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            var responseHttpMessage = starter.CreateCheckStatusResponse(req, instanceId);
            if(responseHttpMessage == null)
            {
                var responseMessage = new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent($"An error occured getting the status of [{orchestratorName}] for instance Id[{instanceId}].") };
                log.LogInformation(await responseMessage.Content.ReadAsStringAsync());
                return responseMessage;
            }

            var content = await responseHttpMessage.Content.ReadAsStringAsync();
            var newContent = $"Started orchestrator [{orchestratorName}] with ID [{instanceId}]\n\n{content}\n\n";
            responseHttpMessage.Content = new StringContent(newContent);
            return responseHttpMessage;
        }
    }
}