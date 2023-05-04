using System;
using System.Net;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Wrap;
using SFA.DAS.EmploymentCheck.Data;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.Services.Hmrc
{
    public class HmrcApiRetryPolicies : IHmrcApiRetryPolicies
    {
        private readonly ILogger<HmrcApiRetryPolicies> _logger;
        private readonly IHmrcApiOptionsRepository _optionsRepository;
        private HmrcApiRateLimiterOptions _settings;
        private readonly ApiRetryOptions _apiRetryOptions;
        private readonly ApiRetryDelaySettings _apiRetryDelaySettings;

        public HmrcApiRetryPolicies(
            ILogger<HmrcApiRetryPolicies> logger,
            IHmrcApiOptionsRepository optionsRepository,
            IOptions<ApiRetryOptions> apiRetryOptions,
            ApiRetryDelaySettings apiRetryDelaySettings)
        {
            _logger = logger;
            _optionsRepository = optionsRepository;
            _apiRetryOptions = apiRetryOptions.Value;
            _apiRetryDelaySettings = apiRetryDelaySettings;
        }

        public async Task<AsyncPolicyWrap> GetAll(Func<Task> onRetry)
        {
            _settings = await _optionsRepository.GetHmrcRateLimiterOptions();

            var tooManyRequestsApiHttpExceptionRetryPolicy = Policy
                .Handle<ApiHttpException>(e =>
                    e.HttpCode == (int)HttpStatusCode.TooManyRequests
                )
                .WaitAndRetryAsync(
                    retryCount: _apiRetryOptions.TooManyRequestsRetryCount,
                    sleepDurationProvider: GetDelayAdjustmentInterval,
                    onRetryAsync: async (exception, ts, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(HmrcApiRetryPolicies)}: [{retryNumber}/{_apiRetryOptions.TooManyRequestsRetryCount}] TooManyRequests error occurred. Retrying after a delay of {_apiRetryDelaySettings.DelayInMs}ms ...");

                        await _optionsRepository.IncreaseDelaySetting(_settings);
                    }
                );

            var unauthorizedAccessExceptionRetryPolicy = Policy
                .Handle<UnauthorizedAccessException>()
                .RetryAsync(
                    retryCount: _apiRetryOptions.TransientErrorRetryCount,
                    onRetryAsync: async (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation($"{nameof(HmrcApiRetryPolicies)}: [{retryNumber}/{_apiRetryOptions.TooManyRequestsRetryCount}] UnauthorizedAccessException occurred. Retrying...");

                        await onRetry();
                    }
                );

            var apiHttpExceptionRetryPolicy = Policy
                .Handle<ApiHttpException>(e =>
                    e.HttpCode != (int)HttpStatusCode.BadRequest &&
                    e.HttpCode != (int)HttpStatusCode.Forbidden &&
                    e.HttpCode != (int)HttpStatusCode.NotFound &&
                    e.HttpCode != (int)HttpStatusCode.TooManyRequests
                )
                .WaitAndRetryAsync(
                    retryCount: _apiRetryOptions.TransientErrorRetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(_apiRetryOptions.TransientErrorDelayInMs),
                    onRetryAsync: async (exception, ts, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(HmrcApiRetryPolicies)}: [{retryNumber}/{_apiRetryOptions.TransientErrorRetryCount}] ApiHttpException occurred. $[{exception}]. Retrying...");

                        await onRetry();
                    }
                );

            return Policy.WrapAsync(tooManyRequestsApiHttpExceptionRetryPolicy, unauthorizedAccessExceptionRetryPolicy, apiHttpExceptionRetryPolicy);
        }

        private TimeSpan GetDelayAdjustmentInterval(int arg)
        {
            _settings = _optionsRepository.GetHmrcRateLimiterOptions().Result;

            return TimeSpan.FromMilliseconds(_apiRetryDelaySettings.DelayInMs);
        }

        public async Task<AsyncPolicy> GetTokenRetrievalRetryPolicy()
        {
            _settings = await _optionsRepository.GetHmrcRateLimiterOptions();

            return await Task.FromResult<AsyncPolicy>(Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: _apiRetryOptions.TokenRetrievalRetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(_settings.TokenFailureRetryDelayInMs),
                    onRetryAsync: (exception, ts, retryNumber, context) =>
                    {
                        _logger.LogInformation($"{nameof(HmrcApiRetryPolicies)}: Exception occurred while retrieving token. Retrying ({retryNumber}/{_apiRetryOptions.TokenRetrievalRetryCount})... {exception}");
                        return Task.CompletedTask;
                    }
                ));
        }

        public async Task ReduceRetryDelay() => await _optionsRepository.ReduceDelaySetting(_settings);
        
        public async Task DelayApiExecutionByRetryPolicy()
        {
            if (_apiRetryDelaySettings.DelayInMs == 0) return;

            _logger.LogInformation($"{nameof(HmrcApiRetryPolicies)}: delaying Hmrc Api call by {_apiRetryDelaySettings.DelayInMs}ms ...");

            await Task.Delay(TimeSpan.FromMilliseconds(_apiRetryDelaySettings.DelayInMs));
        }
    }
}