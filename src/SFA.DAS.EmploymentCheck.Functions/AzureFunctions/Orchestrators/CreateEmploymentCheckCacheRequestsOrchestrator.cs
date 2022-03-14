using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestsOrchestrator
    {
        private const string ThisClassName = nameof(CreateEmploymentCheckCacheRequestsOrchestrator);
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
        public async Task CreateEmploymentCheckRequestsTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckRequestsTask";

            try
            {
                var employmentCheck = await context.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null);
                if (employmentCheck != null)
                {
                    var learnerNiNumberTask = context.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), employmentCheck);
                    var employerPayeSchemesTask = context.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), employmentCheck);

                    await Task.WhenAll(learnerNiNumberTask, employerPayeSchemesTask);
                    var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumberTask.Result, employerPayeSchemesTask.Result);

                    var checkDataValidationStatus = _employmentCheckDataValidator.IsValidEmploymentCheckData(employmentCheckData);
                    if (checkDataValidationStatus.IsValid)
                    {
                        await context.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), employmentCheckData);
                    }
                    else
                    {
                        employmentCheckData.EmploymentCheck.ErrorType = checkDataValidationStatus.ErrorType;
                        await context.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), employmentCheck);
                    }
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(GetEmploymentCheckActivity)} returned no results. Nothing to process.");

                    // No data found so sleep for 10 seconds then execute the orchestrator again (ContinueAsNew() call)
                    var sleep = context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(10));
                    await context.CreateTimer(sleep, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {e.Message}. {e.StackTrace}");
            }
            finally
            {
                context.ContinueAsNew(null);
            }
        }
    }
}