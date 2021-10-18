using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Orchestrators;

namespace SFA.DAS.EmploymentCheck.Functions.Triggers
{
    public static class EmploymentCheckHttpTrigger
    {
        [FunctionName("EmploymentCheckHttpTrigger")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "orchestrators/EmploymentCheckOrchestrator")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            log.LogInformation($"Triggering EmploymentCheckOrchestrator");

            string instanceId = await starter.StartNewAsync(nameof(TestEmploymentCheckOrchestrator), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
