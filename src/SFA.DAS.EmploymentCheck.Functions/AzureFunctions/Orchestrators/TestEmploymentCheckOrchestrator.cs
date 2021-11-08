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
    public class TestEmploymentCheckOrchestrator
    {
        private ILogger<TestEmploymentCheckOrchestrator> _logger;

        public TestEmploymentCheckOrchestrator(ILogger<TestEmploymentCheckOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(TestEmploymentCheckOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = "EmploymentCheckOrchestrator.RunOrchestrator()";
            Log.WriteLog(_logger, thisMethodName, "Started", context);
            Log.WriteLog(_logger, thisMethodName, "Started", context);

            try
            {
                var tasks = new List<Task>();

                // Get the apprentices requiring an employment check
                // Note: We have to await this call as we can't do anything else until we have the list of apprentices
                var apprentices = await context.CallActivityAsync<IList<Apprentice>>(nameof(GetApprenticesActivity), null);

                // Get the apprentices National Insurance Numbers
                // Note: We don't need to await this call as once we have the list of apprentices for it's input it can run independently of anything else
                // until we create the final data model to save to the db queue
                var apprenticesNiNumbers = /* await */ context.CallActivityAsync<IList<ApprenticeNiNumber>>(nameof(GetApprenticesNiNumberActivity), apprentices);

                // Get the apprentices employer PAYE schemes
                // Note: We don't need to await this call as once we have the list of apprentices for it's input it can run independently of anything else
                // until we create the final data model to save to the db queue
                var employersPayeSchemes = /* await */ context.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployersPayeSchemesActivity), apprentices);

                // Note: We need to wait for the NI numbers and PAYE schemes calls to finish before proceeding
                await Task.WhenAll(tasks);

                // TODO: Implement the employment check
                // Check learner employment status
                var temp = new ApprenticeEmploymentParams();
                temp.Apprentices = apprentices;
                // TODO: Get the value of the following without blocking the thread (maybe use ContinueWith delegate?)
                //temp.ApprenticeNiNumbers = apprenticesNiNumbers;
                //temp.EmployerPayeSchemes = employersPayeSchemes;

                var learnersEmploymentStatuses = await context.CallActivityAsync<List<Apprentice>>(nameof(CheckApprenticeEmploymentStatusActivity), new { apprentices, apprenticesNiNumbers, employersPayeSchemes });
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }

            Log.WriteLog(_logger, thisMethodName, "COMPLETED", context);
        }
    }
}
