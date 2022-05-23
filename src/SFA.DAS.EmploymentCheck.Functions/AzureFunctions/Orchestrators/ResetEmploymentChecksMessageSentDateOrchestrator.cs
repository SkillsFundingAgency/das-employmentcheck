using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ResetEmploymentChecksMessageSentDateOrchestrator
    {
        [FunctionName(nameof(ResetEmploymentChecksMessageSentDateOrchestrator))]
        public async Task<string> ResetEmploymentChecksMessageSentDateTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            try
            {
                var employmentCheckMessageSentData = context.GetInput<string>();
                var updatedRowsCount = await context.CallActivityAsync<long>(nameof(ResetEmploymentChecksMessageSentDateActivity), employmentCheckMessageSentData);

                return $"Reset of MessageSentDate completed, {updatedRowsCount} row(s) were updated.";
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}.";
            }
        }
    }
}