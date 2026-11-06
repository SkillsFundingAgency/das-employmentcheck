using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class PurgeInstanceHistoryHttpTrigger
    {
        [FunctionName(nameof(PurgeInstanceHistoryHttpTrigger))]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/PurgeInstancesHttpTrigger")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log
        )
        {
            var allStatuses = Enum.GetValues(typeof(OrchestrationStatus)).Cast<OrchestrationStatus>();
            
            var result = await starter.PurgeInstanceHistoryAsync(DateTime.MinValue, null, allStatuses);
           
            log.LogInformation("PurgeInstanceHistoryHttpTrigger: cleanup done, {InstancesDeleted} instances deleted.",
                result.InstancesDeleted);

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent($"{nameof(PurgeInstanceHistoryHttpTrigger)}: cleanup done, {result.InstancesDeleted} instances deleted.")};
        }
    }
}
