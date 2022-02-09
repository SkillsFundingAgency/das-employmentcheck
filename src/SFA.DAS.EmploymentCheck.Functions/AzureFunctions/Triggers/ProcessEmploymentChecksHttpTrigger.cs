﻿using System;
using System.Linq;
using System.Net;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class ProcessEmploymentChecksHttpTrigger
    {
        private const string InstanceIdPrefix = "ProcessEmploymentCheck-";

        [FunctionName("ProcessApprenticeEmploymentChecksHttpTrigger")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/ProcessApprenticeEmploymentChecksOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var triggerHelper = new TriggerHelper();
            var existingInstances = await triggerHelper.GetRunningInstances(nameof(ProcessEmploymentChecksHttpTrigger),
                InstanceIdPrefix, starter, log);

            if (!existingInstances.DurableOrchestrationState.Any())
            {
                log.LogInformation($"Triggering {nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)}");
                
                string instanceId = await starter.StartNewAsync(nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator), $"{InstanceIdPrefix}{Guid.NewGuid()}");
                
                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
                
                return starter.CreateCheckStatusResponse(req, instanceId);
            }

            return new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent($"An instance of {nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)} is already running."),
            };
        }
    }
}
