using System;
using System.Net;
using System.Threading.Tasks;
using Boxed.AspNetCore;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.Services
{
    public class ApiRetryPolicies : IApiRetryPolicies
    {
        private readonly ILogger<ApiRetryPolicies> _logger;
        private readonly IApiOptionsRepository _optionsRepository;
        private ApiRetryOptions _settings;
        public ApiRetryPolicies(ILogger<ApiRetryPolicies> logger,
            IApiOptionsRepository optionsRepository)
        {
            _logger = logger;
            _optionsRepository = optionsRepository; 
        }

        public async Task<AsyncPolicyWrap> GetAll(Func<Task> onRetry)
        {
            _settings = await _optionsRepository.GetOptions();

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
                            $"{nameof(ApiRetryPolicies)}: [{retryNumber}/{_settings.TooManyRequestsRetryCount}] TooManyRequests error occurred. Retrying after a delay of {_settings.DelayInMs}ms ...");

                        await _optionsRepository.IncreaseDelaySetting(_settings);
                    }
                );

            var unauthorizedAccessExceptionRetryPolicy = Policy
                .Handle<UnauthorizedAccessException>()
                .RetryAsync(
                    retryCount: _settings.TransientErrorRetryCount,
                    onRetryAsync: async (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation($"{nameof(ApiRetryPolicies)}: [{retryNumber}/{_settings.TooManyRequestsRetryCount}] UnauthorizedAccessException occurred. Retrying...");

                        await onRetry();
                    }
                );

            var apiHttpExceptionRetryPolicy = Policy
                .Handle<HttpException>(e =>
                    e.StatusCode != (int)HttpStatusCode.BadRequest &&
                    e.StatusCode != (int)HttpStatusCode.Forbidden &&
                    e.StatusCode != (int)HttpStatusCode.NotFound &&
                    e.StatusCode != (int)HttpStatusCode.TooManyRequests
                )
                .WaitAndRetryAsync(
                    retryCount: _settings.TransientErrorRetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(_settings.TransientErrorDelayInMs),
                    onRetryAsync: async (exception, ts, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(ApiRetryPolicies)}: [{retryNumber}/{_settings.TransientErrorRetryCount}] ApiHttpException occurred. $[{exception}]. Retrying...");

                        await onRetry();
                    }
                );

            return Policy.WrapAsync(tooManyRequestsApiHttpExceptionRetryPolicy, unauthorizedAccessExceptionRetryPolicy, apiHttpExceptionRetryPolicy);
        }

        private TimeSpan GetDelayAdjustmentInterval(int arg)
        {
            _settings = _optionsRepository.GetOptions().Result;

            return TimeSpan.FromMilliseconds(_settings.DelayInMs);
        }

        public async Task<AsyncPolicy> GetRetrievalRetryPolicy()
        {
            _settings = await _optionsRepository.GetOptions();

            return await Task.FromResult<AsyncPolicy>(Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: _settings.TokenRetrievalRetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(_settings.TokenFailureRetryDelayInMs),
                    onRetryAsync: (exception, ts, retryNumber, context) =>
                    {
                        _logger.LogInformation($"{nameof(ApiRetryPolicies)}: Exception occurred while retrieving token. Retrying ({retryNumber}/{_settings.TokenRetrievalRetryCount})... {exception}");
                        return Task.CompletedTask;
                    }
                ));
        }

        public async Task ReduceRetryDelay() => await _optionsRepository.ReduceDelaySetting(_settings);
    }
}
