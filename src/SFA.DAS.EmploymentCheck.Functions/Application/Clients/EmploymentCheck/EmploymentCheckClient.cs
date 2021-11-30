using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public class EmploymentCheckClient : IEmploymentCheckClient
    {
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        private readonly IEmploymentCheckService _employmentCheckService;
        private readonly ILogger<IEmploymentCheckClient> _logger;

        public EmploymentCheckClient(
            IEmploymentCheckService employmentCheckService,
            ILogger<IEmploymentCheckClient> logger)
        {
            _employmentCheckService = employmentCheckService;
            _logger = logger;
        }

        public async Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Client(long employmentCheckLastGetId)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.GetApprenticeEmploymentChecksBatch_Client()";

            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentChecks = null;
            try
            {
                apprenticeEmploymentChecks = await _employmentCheckService.GetApprenticeEmploymentChecksBatch_Service(employmentCheckLastGetId);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return apprenticeEmploymentChecks;
        }

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue
        /// </summary>
        /// <param name="apprenticeEmploymentData"></param>
        /// <returns></returns>
        public async Task EnqueueApprenticeEmploymentCheckMessages_Client(ApprenticeRelatedData apprenticeEmploymentData)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.EnqueueApprenticeEmploymentCheckMessages_Client()";

            try
            {
                if (apprenticeEmploymentData != null)
                {
                    await _employmentCheckService.EnqueueApprenticeEmploymentCheckMessages_Service(apprenticeEmploymentData);
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: The list of enriched employment data (apprentice, NI numbers and Paye Schemes) parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}:{ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API
        /// </summary>
        /// <returns>ApprenticeEmploymentCheckMessageModel</returns>
        public async Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Client()
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.DequeueApprenticeEmploymentCheckMessage_Client()";

            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessage = null;
            try
            {
                apprenticeEmploymentCheckMessage = await _employmentCheckService.DequeueApprenticeEmploymentCheckMessage_Service();

                if(apprenticeEmploymentCheckMessage == null)
                {
                    _logger.LogInformation($"{thisMethodName}: The apprenticeEmploymentCheckMessage value returned from the call to DequeueApprenticeEmploymentCheckMessage_Service() is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return apprenticeEmploymentCheckMessage;
        }

        public async Task SaveApprenticeEmploymentCheckResult_Client(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.SaveApprenticeEmploymentCheckResult_Client()";

            try
            {
                if (apprenticeEmploymentCheckMessageModel != null)
                {
                    await _employmentCheckService.SaveEmploymentCheckResult_Service(apprenticeEmploymentCheckMessageModel);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: The apprenticeEmploymentCheckMessageModel input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
