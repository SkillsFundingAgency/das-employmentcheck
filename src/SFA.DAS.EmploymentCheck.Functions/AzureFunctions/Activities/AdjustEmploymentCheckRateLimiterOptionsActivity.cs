using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class AdjustEmploymentCheckRateLimiterOptionsActivity
    {
        private readonly IHmrcApiOptionsRepository _optionsRepository;
        private readonly ILogger<AdjustEmploymentCheckRateLimiterOptionsActivity> _logger;

        public AdjustEmploymentCheckRateLimiterOptionsActivity(
            IHmrcApiOptionsRepository optionsRepository,
            ILogger<AdjustEmploymentCheckRateLimiterOptionsActivity> logger)
        {
            _optionsRepository = optionsRepository;
            _logger = logger;
        }

        [FunctionName(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity))]
        public async Task<TimeSpan> AdjustEmploymentCheckRateLimiterOptionsActivityTask([ActivityTrigger] ApprenticeEmploymentCheckMessageModel input)
        {
            var thisMethodName = $"{nameof(AdjustEmploymentCheckRateLimiterOptionsActivity)}.AdjustEmploymentCheckRateLimiterOptionsActivity()";

            try
            {
                var options = await _optionsRepository.GetHmrcRateLimiterOptions();

                var tooManyRequests = string.Equals(input.ReturnCode, HttpStatusCode.TooManyRequests.ToString(),
                    StringComparison.InvariantCultureIgnoreCase);

                if (tooManyRequests)
                {
                    options.DelayInMs += options.DelayAdjustmentIntervalInMs;
                    await _optionsRepository.IncreaseDelaySetting(options.DelayInMs);
                }
                else
                {
                    options.DelayInMs -= options.DelayAdjustmentIntervalInMs;
                    await _optionsRepository.ReduceDelaySetting(Math.Max(0, options.DelayInMs));
                }

                return await Task.FromResult(TimeSpan.FromMilliseconds(options.DelayInMs));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
                throw;
            }
        }
    }
}
