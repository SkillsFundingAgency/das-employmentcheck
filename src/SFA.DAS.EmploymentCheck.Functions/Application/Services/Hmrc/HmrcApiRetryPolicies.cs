using System;
using System.Net;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Wrap;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcApiRetryPolicies : IHmrcApiRetryPolicies
    {
        private readonly HmrcApiRetryPolicySettings _settings;
        private readonly ILogger<HmrcApiRetryPolicies> _logger;

        public HmrcApiRetryPolicies(
            ILogger<HmrcApiRetryPolicies> logger,
            IOptions<HmrcApiRetryPolicySettings> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public AsyncPolicyWrap GetAll(Func<Task> onRetry)
        {
            var tooManyRequestsApiHttpExceptionRetryPolicy = Policy
                .Handle<ApiHttpException>(e =>
                    e.HttpCode == (int)HttpStatusCode.TooManyRequests
                )
                .WaitAndRetryAsync(
                    retryCount: _settings.TooManyRequestsRetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(_settings.TransientErrorDelayInMs),
                    onRetryAsync: (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(HmrcApiRetryPolicies)}: TooManyRequests error occurred. Retrying after a delay...");
                        return Task.CompletedTask;
                    }
                );

            var unauthorizedAccessExceptionRetryPolicy = Policy
                .Handle<UnauthorizedAccessException>()
                .RetryAsync(
                    retryCount: _settings.TransientErrorRetryCount,
                    onRetryAsync: async (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(HmrcApiRetryPolicies)}: UnauthorizedAccessException occurred. Refreshing access token...");
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
                            $"{nameof(HmrcApiRetryPolicies)}: ApiHttpException occurred. $[{exception}] Refreshing access token...");
                        await onRetry();
                    }
                );

            return Policy.WrapAsync(tooManyRequestsApiHttpExceptionRetryPolicy, unauthorizedAccessExceptionRetryPolicy, apiHttpExceptionRetryPolicy);
        }
    }
}