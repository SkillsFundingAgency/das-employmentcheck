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
            var checkActivityName = nameof(GetEmploymentCheckActivity);
            var ninoActivityName = nameof(GetLearnerNiNumberActivity);
            var payeActivityName = nameof(GetEmployerPayeSchemesActivity);
            var requestActivityName = nameof(CreateEmploymentCheckCacheRequestActivity);

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");


                var employmentCheck = await context.CallActivityAsync<Models.EmploymentCheck>(checkActivityName, null);
                if (employmentCheck != null)
                {
                    var learnerNiNumberTask = context.CallActivityAsync<LearnerNiNumber>(ninoActivityName, employmentCheck);
                    var employerPayeSchemesTask = context.CallActivityAsync<EmployerPayeSchemes>(payeActivityName, employmentCheck);
                    await Task.WhenAll(learnerNiNumberTask, employerPayeSchemesTask);

                    var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumberTask.Result, employerPayeSchemesTask.Result);
                    await context.CallActivityAsync(requestActivityName, employmentCheckData);
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