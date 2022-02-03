using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
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
                    sleepDurationProvider: await GetDelayAdjustmentInterval(),
                    onRetryAsync: async (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(HmrcApiRetryPolicies)}: TooManyRequests error occurred. Retrying after a delay ({retryNumber}/{_settings.TooManyRequestsRetryCount})...");

                        await _optionsRepository.IncreaseDelaySetting(_settings);
                    }
                );

            var unauthorizedAccessExceptionRetryPolicy = Policy
                .Handle<UnauthorizedAccessException>()
                .RetryAsync(
                    retryCount: _settings.TransientErrorRetryCount,
                    onRetryAsync: async (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(HmrcApiRetryPolicies)}: UnauthorizedAccessException occurred. Refreshing access token ({retryNumber}/{_settings.TransientErrorRetryCount})...");

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

        private async Task<Func<int, TimeSpan>> GetDelayAdjustmentInterval()
        {
            _settings = await _optionsRepository.GetHmrcRateLimiterOptions();
            
            return _ => TimeSpan.FromMilliseconds(_settings.DelayAdjustmentIntervalInMs);
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