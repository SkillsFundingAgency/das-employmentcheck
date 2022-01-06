using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc
{
    public class HmrcClient : IHmrcClient
    {
        #region Private members
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        private readonly IHmrcService _hmrcService;
        private readonly ILogger<IHmrcClient> _logger;
        #endregion Private members

        #region Constructors
        public HmrcClient(IHmrcService  hmrcService, ILogger<HmrcClient> logger)
        {
            _hmrcService = hmrcService;
            _logger = logger;
        }
        #endregion Constructors

        #region CheckEmploymentStatus
        /// <summary>
        /// Sends the employmentCheckCacheRequest to the HMRC API
        /// </summary>
        /// <returns>Task<IList<ApprenticeEmploymentCheck>></returns>
        public async Task<EmploymentCheckCacheRequest> CheckEmploymentStatus(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"{nameof(HmrcClient)}.CheckEmploymentStatus_Client()";

            EmploymentCheckCacheRequest employmentCheckCacheRequestResult = null;
            try
            {
                if (employmentCheckCacheRequest != null)
                {
                    employmentCheckCacheRequestResult = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(employmentCheckCacheRequest);

                    if (employmentCheckCacheRequestResult == null)
                    {
                        _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessageResult value returned from the IsNationalInsuranceNumberRelatedToPayeScheme() call returned null.");
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckMessageResult input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckCacheRequestResult;
        }
        #endregion CheckEmploymentStatus
    }
}
