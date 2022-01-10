using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Application.Common.Models;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class AdjustEmploymentCheckRateLimiterOptionsActivity
    {
        #region Private members
        private readonly IHmrcApiOptionsRepository _optionsRepository;
        private readonly ILogger<AdjustEmploymentCheckRateLimiterOptionsActivity> _logger;
        private readonly HmrcApiRateLimiterOptions _options;
        #endregion Private members

        #region Constructors
        public AdjustEmploymentCheckRateLimiterOptionsActivity(
            ILogger<AdjustEmploymentCheckRateLimiterOptionsActivity> logger,
            IHmrcApiOptionsRepository optionsRepository)
        {
            _logger = logger;
            _optionsRepository = optionsRepository;
            _options = _optionsRepository.GetHmrcRateLimiterOptions().Result;
        }
        #endregion Constructors

        #region AdjustEmploymentCheckRateLimiterOptionsActivityTask
        [FunctionName(nameof(AdjustEmploymentCheckRateLimiterOptionsActivity))]
        public async Task<TimeSpan> AdjustEmploymentCheckRateLimiterOptionsActivityTask([ActivityTrigger] EmploymentCheckCacheRequest input)
        {
            var thisMethodName = $"{nameof(AdjustEmploymentCheckRateLimiterOptionsActivity)}.AdjustEmploymentCheckRateLimiterOptionsActivity()";

            try
            {
                var tooManyRequests = input.RequestCompletionStatus == (short)HttpStatusCode.TooManyRequests;

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
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
                throw;
            }
        }
        #endregion AdjustEmploymentCheckRateLimiterOptionsActivityTask
    }
}