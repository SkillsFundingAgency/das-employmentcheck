using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestOrchestrator
    {
        private const string ThisClassName = nameof(CreateEmploymentCheckCacheRequestsOrchestrator);
        private readonly ILogger<CreateEmploymentCheckCacheRequestOrchestrator> _logger;

        public CreateEmploymentCheckCacheRequestOrchestrator(
            ILogger<CreateEmploymentCheckCacheRequestOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestOrchestrator))]
        public async Task CreateEmploymentCheckCacheRequestTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context
        )
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckCacheRequestTask";

            try
            {
                var employmentCheck = context.GetInput<Models.EmploymentCheck>();
                if (employmentCheck != null)
                {
                    var getLearnerNiNumbersActivityTask = context.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), employmentCheck);
                    var employerPayeSchemesTask = context.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), employmentCheck);

                    await Task.WhenAll(getLearnerNiNumbersActivityTask, employerPayeSchemesTask);

                    await context.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), new EmploymentCheckData(employmentCheck, getLearnerNiNumbersActivityTask.Result, employerPayeSchemesTask.Result));
                }
                else
                {
                    _logger.LogError($"{nameof(CreateEmploymentCheckCacheRequestOrchestrator)}.{nameof(CreateEmploymentCheckCacheRequestTask)}: The input EmploymentCheck parameter was null\n");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {e.Message}. {e.StackTrace}");
            }
        }
    }
}
