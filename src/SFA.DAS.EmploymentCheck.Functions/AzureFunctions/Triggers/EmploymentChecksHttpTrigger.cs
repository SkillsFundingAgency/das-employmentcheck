using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Net;
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
            IEmploymentChecksHttpTriggerHelper employmentChecksHttpTriggerHelper = new EmploymentChecksHttpTriggerHelper();
            return await StartTheCreateAndProcessRequestOrchestrators(req, starter, log, employmentChecksHttpTriggerHelper);
        }

        public static async Task<HttpResponseMessage> StartTheCreateAndProcessRequestOrchestrators(
            HttpRequestMessage req,
            IDurableOrchestrationClient starter,
            ILogger log,
            IEmploymentChecksHttpTriggerHelper employmentChecksHttpTriggerHelper
        )
        {
            var createResult = await employmentChecksHttpTriggerHelper.StartCreateRequestsOrchestrator(req, starter, log);
            if (!createResult.Item2.IsSuccessStatusCode) { return createResult.Item2; }

            var processResult = await employmentChecksHttpTriggerHelper.StartProcessRequestsOrchestrator(req, starter, log);
            if (!processResult.Item2.IsSuccessStatusCode) { return processResult.Item2; }

            var createResultContentString = await employmentChecksHttpTriggerHelper.FormatResponseString(createResult, nameof(CreateEmploymentCheckCacheRequestsOrchestrator));
            var processResultContentString = await employmentChecksHttpTriggerHelper.FormatResponseString(createResult, nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator));

            var resultMessage = await employmentChecksHttpTriggerHelper.CreateHttpResponseMessage(createResultContentString, processResultContentString);

            string gg = resultMessage.ToString();
            _ = gg;

            return resultMessage;
        }
    }
}