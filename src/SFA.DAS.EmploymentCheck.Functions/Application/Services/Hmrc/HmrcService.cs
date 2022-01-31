using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ITokenServiceApiClient _tokenService;
        private readonly ILogger<HmrcService> _logger;
        private readonly IEmploymentCheckCacheResponseRepository _repository;
        private PrivilegedAccessToken _cachedToken;
        private const int UnauthorizedAccessRetryCount = 10;
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public HmrcService(
            ITokenServiceApiClient tokenService,
            IApprenticeshipLevyApiClient apprenticeshipLevyService,
            ILogger<HmrcService> logger,
            IEmploymentCheckCacheResponseRepository repository,
            IEmploymentCheckClient employmentCheckClient
        )
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
            _repository = repository;
            _cachedToken = null;
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<EmploymentCheckCacheRequest> IsNationalInsuranceNumberRelatedToPayeScheme(EmploymentCheckCacheRequest request)
        {
            var thisMethodName = $"{nameof(HmrcService)}.IsNationalInsuranceNumberRelatedToPayeScheme";

            EmploymentCheckCacheResponse employmentCheckCacheResponse = InitialiseEmploymentCheckCacheResponse(request);
            try
            {
                if (_cachedToken == null || AccessTokenHasExpired()) await RetrieveAuthenticationToken();

                var result = await GetEmploymentStatusWithRetries(request, thisMethodName);

                if (result != null)
                {
                    request.Employed = result.Employed;
                    request.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;

                    employmentCheckCacheResponse.Employed = result.Employed;
                    employmentCheckCacheResponse.FoundOnPaye = result.Empref;
                    employmentCheckCacheResponse.HttpResponse = "OK";
                    employmentCheckCacheResponse.HttpStatusCode = (short)HttpStatusCode.OK;
                    await Save(employmentCheckCacheResponse);

                    if (result.Employed)
                    {
                        await SkipRemainingEmploymentStatusChecksForThisApprentice(request);
                    }
                }
                else
                {
                    request.Employed = null;
                    request.RequestCompletionStatus = 500;

                    employmentCheckCacheResponse.HttpResponse = "The response value returned from the HMRC GetEmploymentStatus() call is null.";
                    await Save(employmentCheckCacheResponse);

                    _logger.LogError($"{thisMethodName}: [{employmentCheckCacheResponse.HttpResponse}]");
                }
            }
            catch (ApiHttpException e) when (
                e.HttpCode == (int)HttpStatusCode.TooManyRequests ||
                e.HttpCode == (int)HttpStatusCode.RequestTimeout
                )
            {
                employmentCheckCacheResponse.ProcessingComplete = false;
                employmentCheckCacheResponse.HttpResponse = e.ResourceUri;
                employmentCheckCacheResponse.HttpStatusCode = (short)e.HttpCode;
                await Save(employmentCheckCacheResponse);
            }
            catch (ApiHttpException e)
            {
                _logger.LogError($"{thisMethodName}: ApiHttpException occurred [{e}]");
                employmentCheckCacheResponse.ProcessingComplete = true;
                employmentCheckCacheResponse.HttpResponse = e.ResourceUri;
                employmentCheckCacheResponse.HttpStatusCode = (short)e.HttpCode;
                await Save(employmentCheckCacheResponse);
            }
            catch (Exception e)
            {
                _logger.LogError($"{thisMethodName}: Exception occurred [{e}]");
                employmentCheckCacheResponse.ProcessingComplete = false;
                employmentCheckCacheResponse.HttpResponse = $"{e.Message[Range.EndAt(Math.Min(8000, e.Message.Length))]}";
                employmentCheckCacheResponse.HttpStatusCode = 500;
                await Save(employmentCheckCacheResponse);
            }

            return request;
        }

        private static EmploymentCheckCacheResponse InitialiseEmploymentCheckCacheResponse(EmploymentCheckCacheRequest request)
        {
            // setup a default template 'response' to store the api response
            var employmentCheckCacheResponse = new EmploymentCheckCacheResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    null,           // Employed
                    null,           // FoundOnPayee,
                    true,           // ProcessingComplete
                    1,              // Count
                    string.Empty,   // Response
                    -1,             // HttpStatusCode
                    DateTime.Now,
                    DateTime.Now);
            return employmentCheckCacheResponse;
        }

        private bool AccessTokenHasExpired()
        {
            var expired = _cachedToken.ExpiryTime < DateTime.Now;
            if (expired) _logger.LogInformation($"[{nameof(HmrcService)}] Access Token has expired, retrieving a new token.");
            return expired;
        }

        private async Task<EmploymentStatus> GetEmploymentStatusWithRetries(EmploymentCheckCacheRequest request, string thisMethodName)
        {
            var unauthorizedAccessExceptionRetryPolicy = Policy
                .Handle<UnauthorizedAccessException>()
                .RetryAsync(
                    retryCount: UnauthorizedAccessRetryCount,
                    onRetryAsync: async (outcome, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{thisMethodName}: UnauthorizedAccessException occurred. Refreshing access token...");
                        await RetrieveAuthenticationToken();
                    }
                );

            var unauthorizedApiHttpExceptionRetryPolicy = Policy
                .Handle<ApiHttpException>(e => e.HttpCode == (int)HttpStatusCode.Unauthorized)
                .RetryAsync(
                    retryCount: UnauthorizedAccessRetryCount,
                    onRetryAsync: async (outcome, retryNumber, context) =>
                    {
                        _logger.LogInformation(
                            $"{thisMethodName}: UnauthorizedAccessException occurred. Refreshing access token...");
                        await RetrieveAuthenticationToken();
                    }
                );

            var policyWrap = Policy.WrapAsync(unauthorizedAccessExceptionRetryPolicy, unauthorizedApiHttpExceptionRetryPolicy);

            var result = await policyWrap.ExecuteAsync(() => GetEmploymentStatus(request));
            return result;
        }

        private async Task<EmploymentStatus> GetEmploymentStatus(EmploymentCheckCacheRequest request)
        {
            var employmentStatus = await _apprenticeshipLevyService.GetEmploymentStatus(
                _cachedToken.AccessCode,
                request.PayeScheme,
                request.Nino,
                request.MinDate,
                request.MaxDate
            );

            return employmentStatus;
        }

        private async Task RetrieveAuthenticationToken()
        {
            _cachedToken = await _tokenService.GetPrivilegedAccessTokenAsync();
        }

        private async Task SkipRemainingEmploymentStatusChecksForThisApprentice(EmploymentCheckCacheRequest request)
        {
            await _employmentCheckClient.UpdateRequestCompletionStatusForRelatedEmploymentCheckCacheRequests(request);
        }

        private async Task Save(EmploymentCheckCacheResponse response)
        {
            if (response == null)
            {
                _logger.LogError($"HmrcService.Save(): ERROR: The employmentCheckCacheResponse model is null.");
                return;
            }

            await _repository.InsertOrUpdate(response);
        }
    }
}