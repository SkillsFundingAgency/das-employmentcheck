using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
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
            Log.WriteLog(_logger, thisMethodName, "Started");

            try
            {
                // Get a list apprentices requiring and employment status check
                var apprenticesToCheck = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetApprenticesToCheck), null);

                if (apprenticesToCheck != null && apprenticesToCheck.Count > 0)
                {
                    Log.WriteLog(_logger, thisMethodName, $"GetApprentices() returned {apprenticesToCheck.Count} apprenctice(s)");

                    // Iterate through the list of apprentices to call the HMRC Employment Check API
                    int i = 0;
                    foreach (var apprentice in apprenticesToCheck)
                    {
                        ++i;
                        try
                        {
                            // Call the HMRC Employment Check API to check this apprentice
                            Log.WriteLog(_logger, thisMethodName, $"Checking employment status of learner [{i}] (ULN:{apprentice.ULN}) of [{apprenticesToCheck.Count}]");
                            await context.CallActivityAsync(nameof(CheckApprentice), apprentice);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation($"{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
                        }
                    }
                }
                else
                {
                    Log.WriteLog(_logger, thisMethodName, $"GetApprenticesToCheck() activity returned null/zero apprentices");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }

            Log.WriteLog(_logger, thisMethodName, "COMPLETED");
        }
    }
}
