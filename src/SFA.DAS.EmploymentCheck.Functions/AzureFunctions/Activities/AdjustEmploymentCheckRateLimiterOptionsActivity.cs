using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
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
        private readonly HmrcApiRateLimiterOptions _options;

        public AdjustEmploymentCheckRateLimiterOptionsActivity(
            IHmrcApiOptionsRepository optionsRepository,
            ILogger<AdjustEmploymentCheckRateLimiterOptionsActivity> logger)
        {
            _optionsRepository = optionsRepository;
            _options =  _optionsRepository.GetHmrcRateLimiterOptions().Result;
            _logger = logger;
        }

        [FunctionName(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity))]
        public async Task<TimeSpan> AdjustEmploymentCheckRateLimiterOptionsActivityTask([ActivityTrigger] ApprenticeEmploymentCheckMessageModel input)
        {
            var thisMethodName = $"{nameof(AdjustEmploymentCheckRateLimiterOptionsActivity)}.AdjustEmploymentCheckRateLimiterOptionsActivity()";

            try
            {
                if (input.ReturnCode != null)
                {
                    var tooManyRequests = input.ReturnCode.Contains(HttpStatusCode.TooManyRequests.ToString(),
                        StringComparison.InvariantCultureIgnoreCase);

                    if (tooManyRequests)
                    {
                        await _optionsRepository.IncreaseDelaySetting(_options);
                    }
                    else
                    {
                        await _optionsRepository.ReduceDelaySetting(_options);
                    }
                }

                return await Task.FromResult(TimeSpan.FromMilliseconds(_options.DelayInMs));
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
                throw;
            }
        }
    }
}
