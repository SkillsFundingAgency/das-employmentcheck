using Boxed.AspNetCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Net;
using System.Threading.Tasks;

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

        public async Task<AsyncPolicyWrap> GetAll()
        {
            _settings = _optionsRepository.GetOptions();

            var unauthorizedAccessExceptionRetryPolicy = Policy
                .Handle<UnauthorizedAccessException>()
                .RetryAsync(
                    retryCount: _settings.TransientErrorRetryCount,
                    onRetry: (exception, retryNumber, context) =>
                    {
                        _logger.LogInformation($"{nameof(ApiRetryPolicies)}: [{retryNumber}/{_settings.TooManyRequestsRetryCount}] UnauthorizedAccessException occurred. Retrying...");
                    }
                );

            var apiHttpExceptionRetryPolicy = Policy
                .Handle<HttpException>(e =>
                    e.StatusCode != (int)HttpStatusCode.BadRequest &&
                    e.StatusCode != (int)HttpStatusCode.NotFound
                )
                .WaitAndRetryAsync(
                    retryCount: _settings.TransientErrorRetryCount,
                    sleepDurationProvider: _ => TimeSpan.FromMilliseconds(_settings.TransientErrorDelayInMs),
                    onRetry: (exception, ts, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{nameof(ApiRetryPolicies)}: [{retryNumber}/{_settings.TransientErrorRetryCount}] ApiHttpException occurred. $[{exception}]. Retrying...");
                    }
                );

            return Policy.WrapAsync(unauthorizedAccessExceptionRetryPolicy, apiHttpExceptionRetryPolicy);
        }

    }
}
