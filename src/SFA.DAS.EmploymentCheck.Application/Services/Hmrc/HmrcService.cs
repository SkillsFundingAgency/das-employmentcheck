using System;
using System.Net;
using System.Threading.Tasks;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.Hmrc
{
    public class HmrcService : IHmrcService
    {
        private readonly IHmrcTokenStore _hmrcTokenStore;
        private readonly IApprenticeshipLevyApiClient _apprenticeshipLevyService;
        private readonly ILogger<HmrcService> _logger;
        private readonly IEmploymentCheckService _employmentCheckService;
        private readonly IHmrcApiRetryPolicies _retryPolicies;

        public HmrcService(
            IHmrcTokenStore hmrcTokenStore,
            IApprenticeshipLevyApiClient apprenticeshipLevyService,
            ILogger<HmrcService> logger,
            IEmploymentCheckService employmentCheckService,
            IHmrcApiRetryPolicies retryPolicies)
        {
            _hmrcTokenStore = hmrcTokenStore;
            _apprenticeshipLevyService = apprenticeshipLevyService;
            _logger = logger;
            _employmentCheckService = employmentCheckService;
            _retryPolicies = retryPolicies;
        }

        public async Task<EmploymentCheckCacheRequest> IsNationalInsuranceNumberRelatedToPayeScheme(EmploymentCheckCacheRequest request)
        {
            EmploymentCheckCacheResponse response;

            try
            {
                var result = await GetEmploymentStatusWithRetries(request);

                request.SetEmployed(result.Employed);

                response = EmploymentCheckCacheResponse.CreateSuccessfulCheckResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    result.Employed,
                    result.Empref);

                await _employmentCheckService.StoreCompletedCheck(request, response);
                await _retryPolicies.ReduceRetryDelay();

                return request;
            }
            catch (ApiHttpException e)
            {
                _logger.LogError($"{nameof(HmrcService)}: ApiHttpException occurred [{e}]");

                response = EmploymentCheckCacheResponse.CreateCompleteCheckErrorResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    e.ResourceUri,
                    (short)e.HttpCode);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(HmrcService)}: Exception occurred [{e}]");

                response = EmploymentCheckCacheResponse.CreateCompleteCheckErrorResponse(
                    request.ApprenticeEmploymentCheckId,
                    request.Id,
                    request.CorrelationId,
                    $"{e.Message[Range.EndAt(Math.Min(8000, e.Message.Length))]}",
                    (short)HttpStatusCode.InternalServerError);
            }

            await _employmentCheckService.StoreCompletedCheck(request, response);

            return request;
        }

        private async Task<EmploymentStatus> GetEmploymentStatusWithRetries(EmploymentCheckCacheRequest request)
        {
            var policyWrap = await _retryPolicies.GetAll(() => _hmrcTokenStore.GetTokenAsync(true));

            var result = await policyWrap.ExecuteAsync(() => GetEmploymentStatus(request));

            return result;
        }

        private async Task<EmploymentStatus> GetEmploymentStatus(EmploymentCheckCacheRequest request)
        {
            var accessCode = await _hmrcTokenStore.GetTokenAsync();

            //await _retryPolicies.DelayApiExecutionByRetryPolicy();

            var employmentStatus = await _apprenticeshipLevyService.GetEmploymentStatus(
                accessCode,
                request.PayeScheme,
                request.Nino,
                request.MinDate,
                request.MaxDate
            );

            return employmentStatus;
        }
    }
}