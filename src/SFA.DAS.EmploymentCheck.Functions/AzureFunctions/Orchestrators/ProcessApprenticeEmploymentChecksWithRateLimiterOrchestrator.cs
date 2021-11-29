using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator
    {
        private readonly ILogger<ProcessApprenticeEmploymentChecksOrchestrator> _logger;
        private readonly IHmrcApiOptionsRepository _optionsRepository;

        public ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator(
            ILogger<ProcessApprenticeEmploymentChecksOrchestrator> logger,
            IHmrcApiOptionsRepository optionsRepository)
        {
            _logger = logger;
            _optionsRepository = optionsRepository;
        }

        [FunctionName(nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator))]
        public async Task ProcessApprenticeEmploymentChecksSubOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator)}.ProcessApprenticeEmploymentChecksSubOrchestratorTask()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                var options = _optionsRepository.GetHmrcRateLimiterOptions();

                // Get the next message off the message queue
                var apprenticeEmploymentCheckMessage = await context.CallActivityAsync<ApprenticeEmploymentCheckMessageModel>(nameof(DequeueApprenticeEmploymentCheckMessageActivity), null);

                if (apprenticeEmploymentCheckMessage == null)
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: {nameof(DequeueApprenticeEmploymentCheckMessageActivity)} returned no results. Nothing to process.");
                    return;
                }

                // Do the employment status check on this message
                var result = await context.CallActivityAsync<ApprenticeEmploymentCheckMessageModel>(
                    nameof(CheckApprenticeEmploymentStatusActivity), apprenticeEmploymentCheckMessage);

                AdjustRequestDelayIfRequired(result, options);

                // Save the employment status back to the database
                await context.CallActivityAsync(nameof(SaveApprenticeEmploymentCheckResultActivity), result);

                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Completed.");

                if (!context.IsReplaying)
                    _logger.LogInformation($"{nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator)}: Completed.");

                // Rate limiter delay between each call
                var delay = context.CurrentUtcDateTime.Add(TimeSpan.FromMilliseconds(options.DelayInMs));
                await context.CreateTimer(delay, CancellationToken.None);

                // execute the orchestrator again with a new context to process the next message
                // Note: The orchestrator may have been unloaded from memory whilst the activity
                // functions were running so this could be a new instance of the orchestrator which
                // will run though the table storage 'event sourcing' state.
                context.ContinueAsNew(null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator)} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }

        private void AdjustRequestDelayIfRequired(ApprenticeEmploymentCheckMessageModel result, HmrcApiRateLimiterOptions options)
        {
            var tooManyRequests = string.Equals(result.ReturnCode, HttpStatusCode.TooManyRequests.ToString(),
                StringComparison.InvariantCultureIgnoreCase);

            if (tooManyRequests)
            {
                options.DelayInMs += options.DelayAdjustmentIntervalInMs;
                _optionsRepository.IncreaseDelaySetting(options.DelayInMs);
            }
            else if (options.DelayInMs > options.DelayAdjustmentIntervalInMs)
            {
                options.DelayInMs -= options.DelayAdjustmentIntervalInMs;
                _optionsRepository.ReduceDelaySetting(options.DelayInMs);
            }

        }

    }
}
