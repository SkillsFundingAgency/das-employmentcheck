using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers
{
    public interface IEmploymentChecksHttpTriggerHelper
    {
        Task<Tuple<string, HttpResponseMessage>> StartCreateRequestsOrchestrator(HttpRequestMessage req, IDurableOrchestrationClient starter, ILogger log);

        Task<Tuple<string, HttpResponseMessage>> StartProcessRequestsOrchestrator(HttpRequestMessage req, IDurableOrchestrationClient starter, ILogger log);

        Task<string> FormatResponseString(Tuple<string, HttpResponseMessage> httpResponseMessage, string orchestratorName);

        Task<HttpResponseMessage> CreateHttpResponseMessage(string createResultContentString, string processResultContentString);
    }
}
