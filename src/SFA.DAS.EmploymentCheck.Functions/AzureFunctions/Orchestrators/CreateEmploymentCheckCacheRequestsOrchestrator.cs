using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using System;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateEmploymentCheckCacheRequestsOrchestrator
    {
        private const string ThisClassName = nameof(CreateEmploymentCheckCacheRequestsOrchestrator);
        private readonly ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> _logger;

        public CreateEmploymentCheckCacheRequestsOrchestrator(
            ILogger<CreateEmploymentCheckCacheRequestsOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsOrchestrator))]
        public async Task CreateEmploymentCheckRequestsTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateEmploymentCheckRequestsTask";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                var employmentCheck = await context.CallActivityAsync<Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), new Object());
                if (employmentCheck != null)
                {
                    var activityName = nameof(GetLearnerNiNumberActivity);
                    var getLearnerNiNumbersActivityTask = context.CallActivityAsync<LearnerNiNumber>(activityName, employmentCheck);

                    activityName = nameof(GetEmployerPayeSchemesActivity);
                    var employerPayeSchemesTask = context.CallActivityAsync<EmployerPayeSchemes>(activityName, employmentCheck);

                    await Task.WhenAll(getLearnerNiNumbersActivityTask, employerPayeSchemesTask);
                    await context.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), new EmploymentCheckData(employmentCheck, getLearnerNiNumbersActivityTask.Result, employerPayeSchemesTask.Result));
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{thisMethodName}: Exception caught - {e.Message}. {e.StackTrace}");
            }
            finally
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Completed.");

                context.ContinueAsNew(null);
            }
        }
    }
}
