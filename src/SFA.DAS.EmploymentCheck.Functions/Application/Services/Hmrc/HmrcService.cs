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
using System.Text;
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
            try
            {
                request.EmploymentCheckedDateTime = DateTime.UtcNow;

                if (ValidateRequest(request) == false) return request;

                if (_cachedToken == null) await RetrieveAuthenticationToken();

                var policy = Policy
                    .Handle<ApiHttpException>(e => e.HttpCode == (int) HttpStatusCode.Unauthorized)
                    .RetryAsync(
                        retryCount: 1,
                        onRetryAsync: async (ex, retryNumber, context) =>
                        {
                            _logger.LogError($"HMRC API returned 401 (Unauthorized)");
                            await RetrieveAuthenticationToken();
                        });

                var result = await policy.ExecuteAsync(() => GetEmploymentStatus(request));
                request.IsEmployed = result.Employed;
                request.ReturnCode = "200 (OK)";
            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.NotFound)
            {
                _logger.LogInformation($"HMRC API returned {e.HttpCode} (Not Found)");
                request.IsEmployed = false;
                request.ReturnCode = $"{e.HttpCode} (Not Found)";
                request.ReturnMessage = e.ResourceUri;
            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.TooManyRequests)
            {
                _logger.LogError($"HMRC API returned {e.HttpCode} (Too Many Requests)");
                request.ReturnCode = $"{e.HttpCode} (Too Many Requests)";
                request.ReturnMessage = e.ResourceUri;
            }
            catch (ApiHttpException e) when (e.HttpCode == (int) HttpStatusCode.BadRequest)
            {
                _logger.LogError($"HMRC API returned {e.HttpCode} (Bad Request)");
                request.ReturnCode = $"{e.HttpCode} (Bad Request)";
                request.ReturnMessage = e.ResourceUri;
            }
            catch (ApiHttpException e)
            {
                _logger.LogError($"HMRC API unhandled exception: {e.HttpCode} {e.Message}");
                request.ReturnCode = $"{e.HttpCode} ({(HttpStatusCode) e.HttpCode})";
                request.ReturnMessage = e.ResourceUri;
            }
            catch (Exception e)
            {
                _logger.LogError($"HMRC API unhandled exception: {e.Message} {e.StackTrace}");
                request.ReturnCode = "HMRC API CALL ERROR";
                request.ReturnMessage = e.Message;
            }

            return request;
        }

        private static bool ValidateRequest(ApprenticeEmploymentCheckMessageModel request)
        {
            var valid = true;
            var sb = new StringBuilder();
            if (request.NationalInsuranceNumber == null)
            {
                valid = false;
                sb.Append("NationalInsuranceNumber is null");
            }

            if (request.PayeScheme == null)
            {
                valid = false;
                if (sb.Length > 0) sb.Append("|");
                sb.Append("PayeScheme is null");
            }

            request.ReturnMessage = sb.ToString();

            return valid;
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
