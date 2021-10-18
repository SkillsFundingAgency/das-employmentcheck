using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

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
            var thisMethodName = "***** 1. EmploymentCheckOrchestrator.RunOrchestrator() *****";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
                // Get a list apprentices requiring and employment status check
                var apprenticesToCheck = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetApprenticesToCheck), null);

                if (apprenticesToCheck != null && apprenticesToCheck.Count > 0)
                {
                    _logger.LogInformation($"{messagePrefix} ***** 1.1 GetApprenticesRequiringEmploymentStatusCheck(context)] returned {apprenticesToCheck.Count} apprenctices. *****");

                    // Iterate through the list of apprentices to call the HMRC Employment Check API
                    foreach (var apprentice in apprenticesToCheck)
                    {
                        try
                        {
                            // Call the HMRC Employment Check API to check this apprentice
                            await context.CallActivityAsync(nameof(CheckApprentice), apprentice);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation($"{messagePrefix} ***** 1.2 Exception caught *****. {ex.Message}. {ex.StackTrace}");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{messagePrefix} ***** 1.1 GetApprenticesToCheck() activity returned null/zero apprentices. *****");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} ***** 1. Exception caught. {ex.Message}. {ex.StackTrace} *****");
            }
        }
    }
}
