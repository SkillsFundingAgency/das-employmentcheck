using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class OutputEmploymentCheckResultsStubOrchestrator
    {
        [FunctionName(nameof(OutputEmploymentCheckResultsStubOrchestrator))]
        public async Task OutputEmploymentCheckResultsStubOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            await context.CallActivityAsync(nameof(OutputEmploymentCheckResultsActivityStub), null);
        }
    }
}