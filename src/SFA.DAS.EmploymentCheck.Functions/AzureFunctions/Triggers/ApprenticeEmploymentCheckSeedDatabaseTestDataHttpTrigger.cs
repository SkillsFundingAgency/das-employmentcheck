﻿using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class ApprenticeEmploymentCheckSeedDatabaseTestDataHttpTrigger
    {
        [FunctionName("ApprenticeEmploymentCheckSeedDatabaseTestDataHttpTrigger")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation("Triggering ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator");

            string instanceId = await starter.StartNewAsync(nameof(ApprenticeEmploymentCheckSeedDatabaseTestDataSubOrchestrator), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}