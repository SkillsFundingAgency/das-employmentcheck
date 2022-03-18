using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ResponseOrchestrator
    {
        private readonly ILogger<ResponseOrchestrator> _logger;

        public ResponseOrchestrator(
            ILogger<ResponseOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(ResponseOrchestrator))]
        public async Task ResponseOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                var loop = true;
                while(loop)
                {
                    var employmentCheck = await context.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetResponseEmploymentCheckActivity), null);
                    if (employmentCheck != null)
                    {
                        //await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), employmentCheck);
                    }
                    else
                    {
                        loop = false;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"\n\n{nameof(ResponseOrchestrator)} Exception caught: {e.Message}. {e.StackTrace}");
            }
        }
    }
}