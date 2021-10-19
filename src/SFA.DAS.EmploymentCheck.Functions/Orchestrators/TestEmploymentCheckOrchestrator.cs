using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Orchestrators
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

            try
            {
                /* Strategic Code */
                // Get list of Learners requiring an employment check
                //var learnersRequiringEmploymentCheck = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetLearnersRequiringEmploymentCheck), null);

                // Get learners National Insurance Numbers
                //var learnersNationalInsuranceNumbers = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetLearnersNationalInsuranceNumbers), null);

                // Get learner employer PAYE schemes
                //var employerPayeSchemes = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetEmployerPayeSchemes), null);

                // Check learner employment status
                //var learnersEmploymentStatuses = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetApprenticesToCheck), null);

                // ------------------------------------------------------------------------------------------------------------------------------------------

                /* Original Code */
                // Get a list apprentices requiring an employment check
                var apprenticesToCheck = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetApprenticesToCheck), null);

                if (apprenticesToCheck != null && apprenticesToCheck.Count > 0)
                {
                    Log.WriteLog(_logger, thisMethodName, $"GetApprentices() returned {apprenticesToCheck.Count} apprenctice(s)", context);

                    // Iterate through the list of apprentices to call the HMRC Employment Check API
                    int i = 0;
                    foreach (var apprentice in apprenticesToCheck)
                    {
                        ++i;
                        try
                        {
                            // Call the HMRC Employment Check API to check this apprentice
                            Log.WriteLog(_logger, thisMethodName, $"Checking employment status of learner [{i}] (ULN:{apprentice.ULN}) of [{apprenticesToCheck.Count}]", context);
                            await context.CallActivityAsync(nameof(CheckApprentice), apprentice);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
                        }
                    }
                }
                else
                {
                    Log.WriteLog(_logger, thisMethodName, $"GetApprenticesToCheck() activity returned null/zero apprentices", context);
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }

            Log.WriteLog(_logger, thisMethodName, "COMPLETED", context);
        }
    }
}
