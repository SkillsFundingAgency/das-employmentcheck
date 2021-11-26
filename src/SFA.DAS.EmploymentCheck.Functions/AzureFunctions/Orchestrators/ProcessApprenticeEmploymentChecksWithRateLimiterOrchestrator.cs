using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public async Task ProcessApprenticeEmploymentChecksSubOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"\n\n{nameof(ProcessApprenticeEmploymentChecksWithRateLimiterOrchestrator)}.{nameof(ProcessApprenticeEmploymentChecksSubOrchestratorTask)}()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                var options = _optionsRepository.GetHmrcRateLimiterOptions();

                var apprenticeEmploymentCheckMessages = await GetNextMessagesOffTheQueue(context, options);

                var results = await DoEmploymentStatusChecks(context, apprenticeEmploymentCheckMessages);

                AdjustRequestDelay(results, options);

                await SaveResults(context, results);

                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Completed.");

                // execute the orchestrator again with a new context to process the next message
                // Note: The orchestrator may have been unloaded from memory whilst the activity
                // functions were running so this could be a new instance of the orchestrator which
                // will run though the table storage 'event sourcing' state.
                context.ContinueAsNew(null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }

        private static async Task<IList<EmploymentCheckMessageModel>> GetNextMessagesOffTheQueue(IDurableOrchestrationContext context,
            HmrcApiRateLimiterOptions options)
        {
            var requests = new List<EmploymentCheckMessageModel>();

            for (var i = 0; i < options.BatchSize; i++)
            {
                requests.Add(
                    await context.CallActivityAsync<EmploymentCheckMessageModel>(
                        nameof(DequeueApprenticeEmploymentCheckMessageActivity), null)
                );
            }

            return requests;
        }

        private static async Task<IList<EmploymentCheckMessageModel>> DoEmploymentStatusChecks(IDurableOrchestrationContext context,
            IEnumerable<EmploymentCheckMessageModel> apprenticeEmploymentCheckMessages)
        {
            var results = new List<EmploymentCheckMessageModel>();

            foreach (var message in apprenticeEmploymentCheckMessages)
            {
                results.Add(
                    await context.CallActivityAsync<EmploymentCheckMessageModel>(
                        nameof(CheckApprenticeEmploymentStatusActivity), message));
            }

            return results;
        }

        private void AdjustRequestDelay(IEnumerable<EmploymentCheckMessageModel> results, HmrcApiRateLimiterOptions options)
        {
            var tooManyRequests = results.Any(r => string.Equals(r.ResponseId.ToString(), HttpStatusCode.TooManyRequests.ToString(),
                StringComparison.InvariantCultureIgnoreCase));

            if (tooManyRequests)
            {
                options.DelayInMs += options.DelayAdjustmentIntervalInMs;
            }
            else if (options.DelayInMs > options.DelayAdjustmentIntervalInMs)
            {
                options.DelayInMs -= options.DelayAdjustmentIntervalInMs;
            }

            _optionsRepository.UpdateRequestDelaySetting(options.DelayInMs);
        }

        private static async Task SaveResults(IDurableOrchestrationContext context, IEnumerable<EmploymentCheckMessageModel> results)
        {
            foreach (var result in results)
            {
                await context.CallActivityAsync(nameof(SaveApprenticeEmploymentCheckResultActivity), result);
            }
        }
    }
}
