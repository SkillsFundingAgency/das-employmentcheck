using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public static class OutputEmploymentCheckResultsStubHttpTrigger
    {
        [FunctionName("OutputEmploymentCheckResultsStubHttpTrigger")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post",
                Route = "orchestrators/OutputEmploymentCheckResultsStubOrchestrator")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var instanceId = await starter.StartNewAsync(nameof(OutputEmploymentCheckResultsStubOrchestrator));

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
