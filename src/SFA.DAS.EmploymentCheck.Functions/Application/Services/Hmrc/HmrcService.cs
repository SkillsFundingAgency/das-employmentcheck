using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
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

        #region Constructors
        public HmrcService(
            ITokenServiceApiClient tokenService,
            IApprenticeshipLevyApiClient apprenticeshipLevyService,
            ILogger<HmrcService> logger,
            IEmploymentCheckCacheResponseRepository repository
            )
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
            _repository = repository;
            _cachedToken = null;
        }
        #endregion Constructors

        #region IsNationalInsuranceNumberRelatedToPayeScheme
        public async Task<EmploymentCheckCacheRequest> IsNationalInsuranceNumberRelatedToPayeScheme(
            EmploymentCheckCacheRequest request)
        {
            var thisMethodName = $"{nameof(HmrcService)}.IsNationalInsuranceNumberRelatedToPayeScheme";

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
                    -1);            // HttpStatusCode

            try
            {
                if (_cachedToken == null) await RetrieveAuthenticationToken();

                var policy = Policy
                    .Handle<UnauthorizedAccessException>()
                    .RetryAsync(
                        retryCount: 10,
                        onRetryAsync: async (outcome, retryNumber, context) =>
                        {
                            _logger.LogInformation($"{thisMethodName}: UnauthorizedAccessException occurred. Refreshing access token...");
                            await RetrieveAuthenticationToken();
                        }
                    );

                var result = await policy.ExecuteAsync(() => GetEmploymentStatus(request));

                if (result != null)
                {
                    request.Employed = result.Employed;
                    request.RequestCompletionStatus = 200;

                    employmentCheckCacheResponse.Employed = result.Employed;
                    employmentCheckCacheResponse.FoundOnPaye = result.Empref;
                    employmentCheckCacheResponse.HttpResponse = "OK";
                    employmentCheckCacheResponse.HttpStatusCode = 200;
                    await _repository.Save(employmentCheckCacheResponse);
                }
                else
                {
                    request.Employed = null;
                    request.RequestCompletionStatus = 500;

                    employmentCheckCacheResponse.HttpResponse = "The response value returned from the HMRC GetEmploymentStatus() call is null.";
                    await _repository.Save(employmentCheckCacheResponse);

                    _logger.LogError($"{thisMethodName}: [{employmentCheckCacheResponse.HttpResponse}]");
                }
            }
            catch (ApiHttpException e) when (
                e.HttpCode == (int) HttpStatusCode.TooManyRequests ||
                e.HttpCode == (int)HttpStatusCode.RequestTimeout
                )
            {
                employmentCheckCacheResponse.ProcessingComplete = false;
                employmentCheckCacheResponse.HttpResponse = e.ResourceUri;
                employmentCheckCacheResponse.HttpStatusCode = (short)e.HttpCode;
                await _repository.Save(employmentCheckCacheResponse);
            }
            catch (ApiHttpException e)
            {
                _logger.LogError($"{thisMethodName}: ApiHttpException occurred [{e}]");
                employmentCheckCacheResponse.ProcessingComplete = true;
                employmentCheckCacheResponse.HttpResponse = e.ResourceUri;
                employmentCheckCacheResponse.HttpStatusCode = (short)e.HttpCode;
                await _repository.Save(employmentCheckCacheResponse);
            }
            catch (Exception e)
            {
                _logger.LogError($"{thisMethodName}: Exception occurred [{e}]");
                employmentCheckCacheResponse.ProcessingComplete = false;
                employmentCheckCacheResponse.HttpResponse = $"{e.Message[Range.EndAt(Math.Min(8000, e.Message.Length))]}";
                employmentCheckCacheResponse.HttpStatusCode = 500;
                await _repository.Save(employmentCheckCacheResponse);
            }

            return request;
        }

        #endregion IsNationalInsuranceNumberRelatedToPayeScheme

        #region GetEmploymentStatus

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

        #endregion GetEmploymentStatus

        #region RetrieveAuthenticationToken

        private async Task RetrieveAuthenticationToken()
        {
            _cachedToken = await _tokenService.GetPrivilegedAccessTokenAsync();
        }

        #endregion RetrieveAuthenticationToken
    }
}