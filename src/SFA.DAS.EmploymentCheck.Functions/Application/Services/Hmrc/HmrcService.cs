using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Polly;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
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
        private PrivilegedAccessToken _cachedToken;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, ILogger<HmrcService> logger)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
            _cachedToken = null;
        }

        public async Task<ApprenticeEmploymentCheckMessageModel> IsNationalInsuranceNumberRelatedToPayeScheme(
            ApprenticeEmploymentCheckMessageModel request)
        {
            if (_cachedToken == null) await RetrieveAuthenticationToken();

            try
            {
                var policy = Policy
                    .Handle<UnauthorizedAccessException>()
                    .RetryAsync(
                        retryCount: 1,
                        onRetryAsync: async (outcome, retryNumber, context) => await RetrieveAuthenticationToken());

                request.EmploymentCheckedDateTime = DateTime.UtcNow;

                var result = await policy.ExecuteAsync(() => GetEmploymentStatus(request));
                request.IsEmployed = result.Employed;
                request.ReturnCode = "200 (OK)";
            }
            catch (ApiHttpException e) when (e.HttpCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogInformation($"HMRC API returned {e.HttpCode} (Not Found)");
                request.IsEmployed = false;
                request.ReturnCode = $"{e.HttpCode} (Not Found)";
                request.ReturnMessage = e.ResourceUri;
            }
            catch (ApiHttpException e) when (e.HttpCode == (int)HttpStatusCode.TooManyRequests)
            {
                _logger.LogError($"HMRC API returned {e.HttpCode} (Too Many Requests)");
                request.ReturnCode = $"{e.HttpCode} (Too Many Requests)";
                request.ReturnMessage = e.ResourceUri;
            }
            catch (ApiHttpException e) when (e.HttpCode == (int)HttpStatusCode.BadRequest)
            {
                _logger.LogError($"HMRC API returned {e.HttpCode} (Bad Request)");
                request.ReturnCode = $"{e.HttpCode} (Bad Request)";
                request.ReturnMessage = e.ResourceUri;
            }
            catch (ApiHttpException e)
            {
                _logger.LogError($"HMRC API unhandled exception: {e.HttpCode} {e.Message}");
                request.ReturnCode = $"{e.HttpCode} ({e.ResourceUri})";
                request.ReturnMessage = e.ResourceUri;
            }

            return request;

        }

        private async Task<EmploymentStatus> GetEmploymentStatus(ApprenticeEmploymentCheckMessageModel request)
        {
            return await _apprenticeshipLevyService.GetEmploymentStatus(
                _cachedToken.AccessCode,
                request.PayeScheme,
                request.NationalInsuranceNumber,
                request.StartDateTime,
                request.EndDateTime
            );
        }

        private async Task RetrieveAuthenticationToken() => _cachedToken = await _tokenService.GetPrivilegedAccessTokenAsync();
    }
}
