using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public class EmploymentCheckClient
        : IEmploymentCheckClient
    {
        private const string ThisClassName = "\n\nEmploymentCheckClient";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private IEmploymentCheckService _employmentCheckService;
        private ILogger<IEmploymentCheckClient> _logger;

        /// <summary>
        /// The Client class that calls the employment check services
        /// </summary>
        /// <param name="employmentCheckDbConfiguration"></param>
        /// <param name="employmentCheckService"></param>
        /// <param name="logger"></param>
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
        public async Task<IList<EmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Client(long employmentCheckLastGetId)
        {
            var thisMethodName = $"{ThisClassName}.GetApprenticeEmploymentChecksBatch_Client()";

            IList<EmploymentCheckModel> apprenticeEmploymentChecks = null;
            try
            {
                apprenticeEmploymentChecks = (IList<EmploymentCheckModel>)await _employmentCheckService.GetApprenticeEmploymentChecksBatch_Service(employmentCheckLastGetId);

                if (apprenticeEmploymentChecks == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentChecks value returned from the  GetApprenticeEmploymentChecksBatch_Service() service returned null.");
                }
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
        public async Task EnqueueApprenticeEmploymentCheckMessages_Client(EmploymentCheckData apprenticeEmploymentData)
        {
            var thisMethodName = $"{ThisClassName}.EnqueueApprenticeEmploymentCheckMessages_Client()";

            try
            {
                if (apprenticeEmploymentData != null)
                {
                    await _employmentCheckService.EnqueueApprenticeEmploymentCheckMessages_Service(apprenticeEmploymentData);
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The list of enriched employment data (apprentice, NI numbers and Paye Schemes) parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}:{ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API
        /// </summary>
        /// <returns>ApprenticeEmploymentCheckMessageModel</returns>
        public async Task<EmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Client()
        {
            var thisMethodName = $"{ThisClassName}.DequeueApprenticeEmploymentCheckMessage_Client()";

            EmploymentCheckMessageModel apprenticeEmploymentCheckMessage = null;
            try
            {
                apprenticeEmploymentCheckMessage = await _employmentCheckService.DequeueApprenticeEmploymentCheckMessage_Service();

                if(apprenticeEmploymentCheckMessage == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessage value returned from the call to DequeueApprenticeEmploymentCheckMessage_Service() is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return apprenticeEmploymentCheckMessage;
        }

        public async Task SaveApprenticeEmploymentCheckResult_Client(EmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel)
        {
            var thisMethodName = $"{ThisClassName}.SaveApprenticeEmploymentCheckResult_Client()";

            try
            {
                if (apprenticeEmploymentCheckMessageModel != null)
                {
                    await _employmentCheckService.SaveEmploymentCheckResult_Service(apprenticeEmploymentCheckMessageModel);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: {ErrorMessagePrefix} The apprenticeEmploymentCheckMessageModel input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
