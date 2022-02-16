using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
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
            IList<HttpResponseMessage> resultMessages = new List<HttpResponseMessage>();
            HttpResponseMessage resultMessage;

            resultMessages.Add(await CreateEmploymentCheckRequestsOrchestratorTrigger.HttpStart(req, starter, log));
            resultMessages.Add(await ProcessEmploymentChecksHttpTrigger.HttpStart(req, starter, log));

            var conflict = resultMessages.Any(rm => rm.StatusCode == HttpStatusCode.Conflict);

            if (conflict) { resultMessage = new HttpResponseMessage(HttpStatusCode.Conflict); }
            else { resultMessage = new HttpResponseMessage(HttpStatusCode.Accepted); }

            StringBuilder stringMessage = new StringBuilder();
            foreach (var httpMessage in resultMessages)
            {
                stringMessage.Append(httpMessage.Content);
            }

            resultMessage.Content = new StringContent($"{stringMessage}\n");
            return resultMessage;
        }
    }
}
