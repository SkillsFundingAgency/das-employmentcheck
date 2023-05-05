using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public class HmrcApiOptionsRepository : IHmrcApiOptionsRepository
    {
        private readonly ApiRetryDelaySettings _apiRetryDelaySettings;
        private readonly HmrcApiRateLimiterOptions _hmrcApiRateLimiterOptions;
        private readonly ILogger<HmrcApiOptionsRepository> _logger;
        
        public HmrcApiOptionsRepository(ApiRetryDelaySettings apiRetryDelaySettings,
                                        IOptions<HmrcApiRateLimiterOptions> hmrcApiRateLimiterOptions, 
                                        ILogger<HmrcApiOptionsRepository> logger)
        {
            _apiRetryDelaySettings = apiRetryDelaySettings;
            _hmrcApiRateLimiterOptions = hmrcApiRateLimiterOptions.Value;
            _logger = logger;
        }
        
        public HmrcApiRateLimiterOptions GetHmrcRateLimiterOptions()
        {
            return _hmrcApiRateLimiterOptions;
        }

        public ApiRetryDelaySettings ReduceDelaySetting(HmrcApiRateLimiterOptions options)
        {
            if (_apiRetryDelaySettings.DelayInMs == 0) return _apiRetryDelaySettings;

            var timeSinceLastUpdate = DateTime.UtcNow - _apiRetryDelaySettings.UpdateDateTime;
            if (timeSinceLastUpdate < TimeSpan.FromMinutes(options.MinimumReduceDelayIntervalInMinutes)) return _apiRetryDelaySettings;

            _apiRetryDelaySettings.DelayInMs = Math.Max(0, _apiRetryDelaySettings.DelayInMs - options.DelayAdjustmentIntervalInMs);
            _apiRetryDelaySettings.UpdateDateTime = DateTime.UtcNow;

            _logger.LogInformation("[HmrcApiOptionsRepository] Reducing DelayInMs setting to {0}ms", new { _apiRetryDelaySettings.DelayInMs });

            return _apiRetryDelaySettings;
        }

        public ApiRetryDelaySettings IncreaseDelaySetting(HmrcApiRateLimiterOptions options)
        {
            var timeSinceLastUpdate = DateTime.UtcNow - _apiRetryDelaySettings.UpdateDateTime;
            if (timeSinceLastUpdate < TimeSpan.FromSeconds(options.MinimumIncreaseDelayIntervalInSeconds)) return _apiRetryDelaySettings;

            _apiRetryDelaySettings.DelayInMs += options.DelayAdjustmentIntervalInMs;
            _apiRetryDelaySettings.UpdateDateTime = DateTime.UtcNow;

            _logger.LogInformation("[HmrcApiOptionsRepository] Increasing DelayInMs setting to {0}ms", new { _apiRetryDelaySettings.DelayInMs });

            return _apiRetryDelaySettings;
        }
    }
}