using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ResponseOrchestrator
    {
        [FunctionName(nameof(ResponseOrchestrator))]
        public async Task ResponseOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var loop = true;
            while (loop)
            {
                var employmentCheck = await context.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetResponseEmploymentCheckActivity), null);
                if (employmentCheck != null)
                {
                    await context.CallActivityAsync(nameof(GetResponseEmploymentCheckActivity), employmentCheck);
                }
                else
                {
                    loop = false;
                }
            }
        }
    }
}