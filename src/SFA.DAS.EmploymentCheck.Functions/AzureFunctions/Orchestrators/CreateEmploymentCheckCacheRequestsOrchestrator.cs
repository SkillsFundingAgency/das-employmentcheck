using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestsOrchestrator
    {
        private readonly ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> _logger;
        private readonly IEmploymentCheckDataValidator _employmentCheckDataValidator;

        public CreateEmploymentCheckCacheRequestsOrchestrator(
            ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> logger,
            IEmploymentCheckDataValidator employmentCheckDataValidator
        )
        {
            _logger = logger;
            _employmentCheckDataValidator = employmentCheckDataValidator;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsOrchestrator))]
        public async Task CreateEmploymentCheckRequestsTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            Data.Models.EmploymentCheck check;

            do
            {
                check = await context.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null);

                await EnrichEmploymentCheckData(context, check);

            } while (check != null);

            _logger.LogInformation(
                $"\n\n{nameof(CreateEmploymentCheckCacheRequestsOrchestrator)}: {nameof(GetEmploymentCheckActivity)} returned no results. Nothing to process.");

        }

        private async Task EnrichEmploymentCheckData(IDurableOrchestrationContext context, Data.Models.EmploymentCheck check)
        {
            if (check == null) return;

            var learnerNiNumberTask = context.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), check);
            var employerPayeSchemesTask = context.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), check);

            await Task.WhenAll(learnerNiNumberTask, employerPayeSchemesTask);

            var result = new EmploymentCheckData(check, learnerNiNumberTask.Result, employerPayeSchemesTask.Result);

            var errors = _employmentCheckDataValidator.EmploymentCheckDataHasError(result);

            if (string.IsNullOrEmpty(errors))
            {
                await context.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), result);
            }
            else
            {
                result.EmploymentCheck.ErrorType = errors;
                await context.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), result);
            }
        }
    }
}