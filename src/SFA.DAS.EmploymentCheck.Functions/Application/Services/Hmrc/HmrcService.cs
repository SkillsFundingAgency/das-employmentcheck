using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private const string ThisClassName = "\n\nHmrcService";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ITokenServiceApiClient _tokenService;
        private readonly ILogger<HmrcService> _logger;
        private PrivilegedAccessToken _cachedToken;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, ILogger<HmrcService> logger)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
            _cachedToken = null;
        }

        public async Task<EmploymentCheckMessage> IsNationalInsuranceNumberRelatedToPayeScheme(
            EmploymentCheckMessage request)
        {
            var thisMethodName = $"{ThisClassName}.IsNationalInsuranceNumberRelatedToPayeScheme()";

            try
            {
                if (_cachedToken == null)
                    await RetrieveAuthenticationToken();

                var policy = Policy
                    .Handle<UnauthorizedAccessException>()
                    .RetryAsync(
                        retryCount: 1,
                        onRetryAsync: async (outcome, retryNumber, context) => await RetrieveAuthenticationToken());

                request.LastEmploymentCheck = DateTime.UtcNow;

                var result = await policy.ExecuteAsync(() => GetEmploymentStatus(request));
                request.Employed = result.Employed;
                request.ResponseHttpStatusCode = 200;
                request.ResponseMessage = "OK";
            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.NotFound)
            {
                _logger.LogInformation($"HMRC API returned {e.HttpCode} (Not Found)");
                request.Employed = false;
                request.ResponseHttpStatusCode = (short) e.HttpCode; // storing as a short in the db to save space as the highest code is only 3 digits
                request.ResponseMessage = $"(Not Found ){e.ResourceUri}";

            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.TooManyRequests)
            {
                _logger.LogError($"HMRC API returned {e.HttpCode} (Too Many Requests)");
                request.ResponseHttpStatusCode = (short)e.HttpCode;
                request.ResponseMessage = $"(Too Many Requests) {e.ResourceUri}";
            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.BadRequest)
            {
                _logger.LogError("HMRC API returned {e.HttpCode} (Bad Request)");
                request.ResponseHttpStatusCode = (short)e.HttpCode;
                request.ResponseMessage = $"(Bad Request) {e.ResourceUri}";
            }

            catch (ApiHttpException e)
            {
                _logger.LogError($"HMRC API unhandled exception: {e.HttpCode} {e.Message}");
                request.ResponseHttpStatusCode = (short)e.HttpCode;
                request.ResponseMessage = $"{e.HttpCode} ({(HttpStatusCode)e.HttpCode} {e.ResourceUri})";
            }
            catch (Exception e)
            {
                _logger.LogError($"HMRC API unhandled exception: {e.Message} {e.StackTrace}");
                request.ResponseHttpStatusCode = 500; // TODO: There is no http status code in the exception so just made this up
                request.ResponseMessage = $"HMRC API CALL ERROR {e.Message}";
            }

            return request;

        }

        private async Task<EmploymentStatus> GetEmploymentStatus(EmploymentCheckMessage request)
        {
            var thisMethodName = $"{ThisClassName}.GetEmploymentStatus()";

            EmploymentStatus employmentStatus = null;
            try
            {
                employmentStatus = await _apprenticeshipLevyService.GetEmploymentStatus(
                    _cachedToken.AccessCode,
                    request.PayeScheme,
                    request.NationalInsuranceNumber,
                    request.MinDateTime,
                    request.MaxDateTime
                );
            }
            catch (Exception ex)
            {
                // All exceptions must be caught and handled because the orchestrator call to the Activity function that is running this method will 'hang' if an exception is not caught and handled.
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employmentStatus;
        }

        private async Task RetrieveAuthenticationToken()
        {
            var thisMethodName = $"{ThisClassName}.RetrieveAuthenticationToken()";

            try
            {
                _cachedToken = await _tokenService.GetPrivilegedAccessTokenAsync();
            }
            catch (Exception ex)
            {
                // All exceptions must be caught and handled because the orchestrator call to the Activity function that is running this method will 'hang' if an exception is not caught and handled.
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
