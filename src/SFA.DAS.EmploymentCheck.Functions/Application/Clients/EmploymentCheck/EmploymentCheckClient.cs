using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
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

        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database
        /// </summary>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public async Task<IList<Models.Domain.EmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Client(long employmentCheckLastGetId)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.GetApprenticeEmploymentChecksBatch_Client()";

            IList<Models.Domain.EmploymentCheckModel> apprenticeEmploymentChecks = null;
            try
            {
                apprenticeEmploymentChecks = (IList<Models.Domain.EmploymentCheckModel>)await _employmentCheckService.GetApprenticeEmploymentChecksBatch_Service(employmentCheckLastGetId);

                if (apprenticeEmploymentChecks == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentChecks value returned from the  GetApprenticeEmploymentChecksBatch_Service() service returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return apprenticeEmploymentChecks;
        }

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue
        /// </summary>
        /// <param name="employmentCheckData"></param>
        /// <returns></returns>
        public async Task EnqueueApprenticeEmploymentCheckMessages_Client(EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.EnqueueApprenticeEmploymentCheckMessages_Client()";

            try
            {
                if (employmentCheckData != null)
                {
                    await _employmentCheckService.EnqueueApprenticeEmploymentCheckMessages_Service(employmentCheckData);
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
        public async Task<EmploymentCheckMessage> DequeueApprenticeEmploymentCheckMessage_Client()
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.DequeueApprenticeEmploymentCheckMessage_Client()";

            EmploymentCheckMessage apprenticeEmploymentCheckMessage = null;
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

        public async Task SaveApprenticeEmploymentCheckResult_Client(EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.SaveApprenticeEmploymentCheckResult_Client()";

            try
            {
                if (employmentCheckMessage != null)
                {
                    await _employmentCheckService.SaveEmploymentCheckResult_Service(employmentCheckMessage);
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
