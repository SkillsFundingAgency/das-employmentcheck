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
            var thisMethodName = "\n\n***** EmploymentCheckOrchestrator.RunOrchestrator() *****";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            _logger.LogInformation($"{messagePrefix}\n----------------------------------------------------------------------\n------ EmploymentCheckOrchestrator.RunOrchestrator() STARTED -------\n----------------------------------------------------------------------\n\n");

            try
            {
                // Get a list apprentices requiring and employment status check
                var apprenticesToCheck = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetApprenticesToCheck), null);

                if (apprenticesToCheck != null && apprenticesToCheck.Count > 0)
                {
                    _logger.LogInformation($"{messagePrefix}\n----------------------------------------------------------------------\n------ GetApprentices() returned {apprenticesToCheck.Count} apprenctice(s) -------\n----------------------------------------------------------------------\n\n");

                    // Iterate through the list of apprentices to call the HMRC Employment Check API
                    int i = 0;
                    foreach (var apprentice in apprenticesToCheck)
                    {
                        ++i;
                        try
                        {
                            // Call the HMRC Employment Check API to check this apprentice
                            //_logger.LogInformation($"{messagePrefix} Checking learner {i} (ULN:{apprentice.ULN}) of {apprenticesToCheck.Count}");
                            await context.CallActivityAsync(nameof(CheckApprentice), apprentice);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogInformation($"{messagePrefix}\n----------------------------------------------------------------------\n------  Exception caught: {ex.Message}. {ex.StackTrace} -------\n----------------------------------------------------------------------\n\n");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{messagePrefix}\n----------------------------------------------------------------------\n------  GetApprenticesToCheck() activity returned null/zero apprentices -------\n----------------------------------------------------------------------\n\n");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix}\n----------------------------------------------------------------------\n------  Exception caught: {ex.Message}. {ex.StackTrace} -------\n----------------------------------------------------------------------\n\n");
            }

            _logger.LogInformation($"{messagePrefix}\n----------------------------------------------------------------------\n------ EmploymentCheckOrchestrator.RunOrchestrator() COMPLETED -------\n----------------------------------------------------------------------\n\n");
        }
    }
}
