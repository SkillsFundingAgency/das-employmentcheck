using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.TokenService.Api.Client;
using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ITokenServiceApiClient _tokenService;
        private readonly ILogger<HmrcService> _logger;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, ILogger<HmrcService> logger)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
        }

        public async Task<ApprenticeEmploymentCheckMessageModel> IsNationalInsuranceNumberRelatedToPayeScheme(
            ApprenticeEmploymentCheckMessageModel request)
        {
            var token = await _tokenService.GetPrivilegedAccessTokenAsync();

            try
            {
                request.EmploymentCheckedDateTime = DateTime.UtcNow;

                var result = await _apprenticeshipLevyService.GetEmploymentStatus(
                    token.AccessCode,
                    request.PayeScheme,
                    request.NationalInsuranceNumber,
                    request.StartDateTime,
                    request.EndDateTime
                );

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
                _logger.LogError("HMRC API returned {e.HttpCode} (Bad Request)");
                request.ReturnCode = $"{e.HttpCode} (Bad Request)";
                request.ReturnMessage = e.ResourceUri;
            }

            return request;
        }
    }
}
