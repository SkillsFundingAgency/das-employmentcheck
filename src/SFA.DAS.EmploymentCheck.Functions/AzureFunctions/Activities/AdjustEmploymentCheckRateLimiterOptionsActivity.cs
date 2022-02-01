using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
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
        private readonly HmrcApiRateLimiterOptions _options;

        public AdjustEmploymentCheckRateLimiterOptionsActivity(
            IHmrcApiOptionsRepository optionsRepository)
        {
            _optionsRepository = optionsRepository;
            _options = _optionsRepository.GetHmrcRateLimiterOptions().Result;
        }

        [FunctionName(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity))]
        public async Task<TimeSpan> AdjustEmploymentCheckRateLimiterOptionsActivityTask([ActivityTrigger] EmploymentCheckCacheRequest input)
        {
            var tooManyRequests = input.RequestCompletionStatus == (short) HttpStatusCode.TooManyRequests;

            if (tooManyRequests)
            {
                await _optionsRepository.IncreaseDelaySetting(_options);
            }
            else
            {
                await _optionsRepository.ReduceDelaySetting(_options);
            }

            return await Task.FromResult(TimeSpan.FromMilliseconds(_options.DelayInMs));
        }
    }
}