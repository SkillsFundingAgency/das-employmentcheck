//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.DurableTask;
//using Microsoft.Extensions.Logging;
//using SFA.DAS.EmploymentCheck.Functions.Activities;
//using SFA.DAS.EmploymentCheck.Functions.Dtos;

//namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
//{
//    public class EmploymentCheckOrchestrator
//    {
//        private ILoggerAdapter<EmploymentCheckOrchestrator> _logger;

//        public EmploymentCheckOrchestrator(ILoggerAdapter<EmploymentCheckOrchestrator> logger)
//        {
//            _logger = logger;
//        }

//        [FunctionName(nameof(EmploymentCheckOrchestrator))]
//        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
//        {
//            var thisMethodName = "EmploymentCheckOrchestrator.RunOrchestrator()";
//            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

//            if (!context.IsReplaying)
//                _logger.LogInformation($"{messagePrefix} Started.");

//            try
//            {
//                _logger.LogInformation($"{messagePrefix} Executing [GetApprenticesToCheck] activity.");
//                var apprenticesToCheck = await context.CallActivityAsync<List<ApprenticeToVerifyDto>>(nameof(GetApprenticesToCheck), null);

//                if (apprenticesToCheck != null && apprenticesToCheck.Count > 0)
//                {
//                    _logger.LogInformation($"{messagePrefix} [GetApprenticesRequiringEmploymentStatusCheck(context)] returned {apprenticesToCheck.Count} apprenctices.");

//                    _logger.LogInformation($"{messagePrefix} Entering Foreach loop to iterate through the list of apprentices to check their employment status.");
//                    foreach (var apprentice in apprenticesToCheck)
//                    {
//                        try
//                        {
//                            _logger.LogInformation($"{messagePrefix} Executing CheckApprentice activity for apprentice {apprentice.Id}.");

//                            await context.CallActivityWithRetryAsync(nameof(CheckApprentice), new RetryOptions(new TimeSpan(0, 0, 0, 10), 6), apprentice);
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogInformation($"{messagePrefix} Exception caught. {ex.Message}. {ex.StackTrace}");
//                        }
//                    }
//                }
//                else
//                {
//                    _logger.LogInformation($"{messagePrefix} [GetApprenticesToCheck] activity returned null/zero apprentices.");
//                }
//            }
//            catch(Exception ex)
//            {
//                _logger.LogInformation($"{messagePrefix} Exception caught. {ex.Message}. {ex.StackTrace}");
//            }

//            _logger.LogInformation($"{messagePrefix} Completed.");
//        }
//    }
//}
