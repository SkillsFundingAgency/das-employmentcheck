using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class CreateApprenticeEmploymentChecksOrchestrator
    {
        private const string ThisClassName = "\n\nCreateApprenticeEmploymentChecksOrchestrator";

        private ILogger<CreateApprenticeEmploymentChecksOrchestrator> _logger;

        public CreateApprenticeEmploymentChecksOrchestrator(
            ILogger<CreateApprenticeEmploymentChecksOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(CreateApprenticeEmploymentChecksOrchestrator))]
        public async Task CreateApprenticeEmploymentChecksSubOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.CreateApprenticeEmploymentChecksSubOrchestratorTask()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Started.");

                // Get the apprentices requiring an employment check (we have to await this call as we can't do anything else until we have the list of apprentices)
                var apprenticeEmploymentChecks = await context.CallActivityAsync<IList<ApprenticeEmploymentCheckModel>>(nameof(GetApprenticeEmploymentChecksActivity), null);

                // Get the apprentices National Insurance Numbers
                var getNationalInsuranceNumbersTask = context.CallActivityAsync<IList<ApprenticeNiNumber>>(nameof(GetApprenticesNiNumberActivity), apprenticeEmploymentChecks);

                // Get the apprentices employer PAYE schemes
                var getPayeSchemesTask = context.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployersPayeSchemesActivity), apprenticeEmploymentChecks);

                // Wait for the NI numbers and PAYE schemes calls to finish before proceeding to add the data to the db message queue
                await Task.WhenAll(getNationalInsuranceNumbersTask, getPayeSchemesTask);

                // We now have all the data we need for the employment check so create a message on the message queue ready for the employment check orchestrator to process
                await context.CallActivityAsync<int>(nameof(EnqueueApprenticeEmploymentCheckMessagesActivity), new ApprenticeRelatedData(apprenticeEmploymentChecks, getNationalInsuranceNumbersTask.Result, getPayeSchemesTask.Result));

                context.ContinueAsNew(null);

                if (!context.IsReplaying)
                    _logger.LogInformation($"\n\n{thisMethodName}: Completed.");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }

        }
    }
}
