﻿using System.Linq;
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
        private const string InstanceIdPrefix = "EmploymentCheck-";

        [FunctionName("ProcessApprenticeEmploymentChecksHttpTrigger")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/ProcessApprenticeEmploymentChecksOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("Checking for running instances of ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator");

            var existingInstances = await starter.ListInstancesAsync(new OrchestrationStatusQueryCondition
            {
                InstanceIdPrefix = InstanceIdPrefix,
                RuntimeStatus = new[]
                {
                    OrchestrationRuntimeStatus.Pending,
                    OrchestrationRuntimeStatus.Running,
                    OrchestrationRuntimeStatus.ContinuedAsNew
                }
            }, System.Threading.CancellationToken.None);

            if (!existingInstances.DurableOrchestrationState.Any())
            {
                log.LogInformation($"Triggering {nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)}");
                
                string instanceId = await starter.StartNewAsync(nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator), null);
                
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
