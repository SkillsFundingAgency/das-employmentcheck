using System;
using System.Net;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.Services.Hmrc
{
    public class HmrcApiRetryPolicies : IHmrcApiRetryPolicies
    {
        private readonly ILogger<HmrcApiRetryPolicies> _logger;
        private readonly IHmrcApiOptionsRepository _optionsRepository;
        private HmrcApiRateLimiterOptions _settings;

        public HmrcApiRetryPolicies(
            ILogger<HmrcApiRetryPolicies> logger,
            IHmrcApiOptionsRepository optionsRepository)
        {
            _logger = logger;
            _optionsRepository = optionsRepository;
        }

        public async Task<AsyncPolicyWrap> GetAll(Func<Task> onRetry)
        {
            _settings = await _optionsRepository.GetHmrcRateLimiterOptions();

            var tooManyRequestsApiHttpExceptionRetryPolicy = Policy
                .Handle<ApiHttpException>(e =>
                    e.HttpCode == (int)HttpStatusCode.TooManyRequests
                )
                .WaitAndRetryAsync(
                    retryCount: _settings.TooManyRequestsRetryCount,
                    sleepDurationProvider: GetDelayAdjustmentInterval,
                    onRetryAsync: async (exception, ts, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(HmrcApiRetryPolicies)}: [{retryNumber}/{_settings.TooManyRequestsRetryCount}] TooManyRequests error occurred. Retrying after a delay of {_settings.DelayInMs}ms ...");

                        await _optionsRepository.IncreaseDelaySetting(_settings);
                    }
                );

            var unauthorizedAccessExceptionRetryPolicy = Policy
                .Handle<UnauthorizedAccessException>()
                .RetryAsync(
                    retryCount: _settings.TransientErrorRetryCount,
                    onRetryAsync: async (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation($"{nameof(HmrcApiRetryPolicies)}: [{retryNumber}/{_settings.TooManyRequestsRetryCount}] UnauthorizedAccessException occurred. Refreshing access token and retrying...");

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
                    retryCount: _settings.TransientErrorRetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(_settings.TransientErrorDelayInMs),
                    onRetryAsync: async (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(HmrcApiRetryPolicies)}: ApiHttpException occurred. $[{exception}] Refreshing access token ({retryNumber}/{_settings.TransientErrorRetryCount})...");

                        await onRetry();
                    }
                );

            return Policy.WrapAsync(tooManyRequestsApiHttpExceptionRetryPolicy, unauthorizedAccessExceptionRetryPolicy, apiHttpExceptionRetryPolicy);
        }

        private TimeSpan GetDelayAdjustmentInterval(int arg)
        {
            _settings = _optionsRepository.GetHmrcRateLimiterOptions().Result;

            return TimeSpan.FromMilliseconds(_settings.DelayInMs);
        }

        public async Task<AsyncPolicy> GetTokenRetrievalRetryPolicy()
        {
            _settings = await _optionsRepository.GetHmrcRateLimiterOptions();

            return await Task.FromResult<AsyncPolicy>(Policy
                .Handle<Exception>()
                .RetryAsync(
                    retryCount: _settings.TokenRetrievalRetryCount,
                    onRetryAsync: (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation($"{nameof(HmrcApiRetryPolicies)}: Exception error occurred while retrieving token. Retrying ({retryNumber}/{_settings.TokenRetrievalRetryCount})...");
                        return Task.CompletedTask;
                    }
                ));
        }

        public async Task ReduceRetryDelay() => await _optionsRepository.ReduceDelaySetting(_settings);
    }
}