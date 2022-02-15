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
            [OrchestrationTrigger] IDurableOrchestrationContext context
        )
        {
            var employmentCheck = context.GetInput<Models.EmploymentCheck>();
            if(employmentCheck != null)
            {
                var employmentChecks = new List<Models.EmploymentCheck>();
                employmentChecks.Add(employmentCheck);

                var getLearnerNiNumbersActivityTask = context.CallActivityAsync<IList<LearnerNiNumber>>(nameof(GetLearnerNiNumbersActivity), employmentChecks);
                var employerPayeSchemesTask = context.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), employmentChecks);

                await Task.WhenAll(getLearnerNiNumbersActivityTask, employerPayeSchemesTask);
                await context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(CreateEmploymentCheckCacheRequestsActivity), new EmploymentCheckData(employmentChecks, getLearnerNiNumbersActivityTask.Result, employerPayeSchemesTask.Result));
            }
        }
    }
}
