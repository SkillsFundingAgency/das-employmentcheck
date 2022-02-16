using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestOrchestrator
    {
        [FunctionName(nameof(CreateEmploymentCheckCacheRequestOrchestrator))]
        public async Task CreateEmploymentCheckCacheRequestTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            Models.EmploymentCheck check
        )
        {
            var employmentCheck = context.GetInput<Models.EmploymentCheck>();
            if(employmentCheck != null)
            {
                var getLearnerNiNumbersActivityTask = context.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumbersActivity), employmentCheck);
                var employerPayeSchemesTask = context.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), employmentCheck);

                await Task.WhenAll(getLearnerNiNumbersActivityTask, employerPayeSchemesTask);

                await context.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestsActivity), new EmploymentCheckData(employmentCheck, getLearnerNiNumbersActivityTask.Result, employerPayeSchemesTask.Result));
            }
            else
            {
                // log something??
            }
        }
    }
}
