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
        private const string ThisClassName = "\n\nHmrcService";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ITokenServiceApiClient _tokenService;
        private readonly ILogger<HmrcService> _logger;

        public HmrcService(ITokenServiceApiClient tokenService, IApprenticeshipLevyApiClient apprenticeshipLevyService, ILogger<HmrcService> logger)
        {
            _tokenService = tokenService;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
        }

        public async Task<EmploymentCheckMessageModel> IsNationalInsuranceNumberRelatedToPayeScheme(
            EmploymentCheckMessageModel request)
        {
            var thisMethodName = $"{ThisClassName}.IsNationalInsuranceNumberRelatedToPayeScheme()";

            var token = await _tokenService.GetPrivilegedAccessTokenAsync();

            try
            {
                request.EmploymentCheckedDateTime = DateTime.UtcNow;

                var result = await _apprenticeshipLevyService.GetEmploymentStatus(
                    token.AccessCode,
                    request.PayeScheme,
                    request.NationalInsuranceNumber,
                    request.MinDateTime,
                    request.MaxDateTime
                );

                request.IsEmployed = result.Employed;
                request.ResponseId = 200;
                request.ResponseMessage = "OK";
            }
            catch (ApiHttpException e) when (e.HttpCode == (int)HttpStatusCode.NotFound)
            {
                _logger.LogInformation($"HMRC API returned {e.HttpCode} (Not Found)");
                request.IsEmployed = false;
                request.ResponseId = (short) e.HttpCode; // storing as a short in the db to save space as the highest code is only 3 digits
                request.ResponseMessage = $"(Not Found ){e.ResourceUri}";

            }
            catch (ApiHttpException e) when (e.HttpCode == (int)HttpStatusCode.TooManyRequests)
            {
                _logger.LogError($"HMRC API returned {e.HttpCode} (Too Many Requests)");
                request.ResponseId = (short)e.HttpCode;
                request.ResponseMessage = $"(Too Many Requests) {e.ResourceUri}";
            }
            catch (ApiHttpException e) when (e.HttpCode == (int)HttpStatusCode.BadRequest)
            {
                _logger.LogError("HMRC API returned {e.HttpCode} (Bad Request)");
                request.ResponseId = (short)e.HttpCode;
                request.ResponseMessage = $"(Bad Request) {e.ResourceUri}";
            }

            catch (Exception ex)
            {
                // All exceptions must be caught and handled because the orchestrator call to the Activity function that is running this method will 'hang' if an exception is not caught and handled.
                _logger.LogError($"{ thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return request;
        }
    }
}
