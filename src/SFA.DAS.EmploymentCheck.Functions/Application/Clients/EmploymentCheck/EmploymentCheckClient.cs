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
        /// Gets a batch of the employment checks from the Employment Check database
        /// </summary>
        /// <returns>Task<IList<EmploymentCheckModel>></returns>
        public async Task<IList<Models.Domain.EmploymentCheckModel>> GetEmploymentChecksBatch_Client(long employmentCheckLastHighestBatchId)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.GetEmploymentChecksBatch_Client()";

            IList<Models.Domain.EmploymentCheckModel> employmentCheckModels = null;
            try
            {
                employmentCheckModels = (IList<Models.Domain.EmploymentCheckModel>)await _employmentCheckService.GetEmploymentChecksBatch_Service(employmentCheckLastHighestBatchId);

                if (employmentCheckModels == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckModels value returned from the GetEmploymentChecksBatch_Service() service returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckModels;
        }

        /// <summary>
        /// Adds an employment check message to the HMRC API message queue
        /// </summary>
        /// <param name="employmentCheckData"></param>
        /// <returns></returns>
        public async Task EnqueueEmploymentCheckMessages_Client(EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.EnqueueEmploymentCheckMessages_Client()";

            try
            {
                if (employmentCheckData != null)
                {
                    await _employmentCheckService.EnqueueEmploymentCheckMessages_Service(employmentCheckData);
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
        /// Gets an employment check message from the HMRC API message queue
        /// </summary>
        /// <returns>EmploymentCheckMessage</returns>
        public async Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Client()
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.DequeueEmploymentCheckMessage_Client()";

            EmploymentCheckMessage employmentCheckMessage = null;
            try
            {
                employmentCheckMessage = await _employmentCheckService.DequeueEmploymentCheckMessage_Service();

                if(employmentCheckMessage == null)
                {
                    _logger.LogInformation($"{thisMethodName}: The employmentCheckMessage value returned from the call to DequeueEmploymentCheckMessage_Service() is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckMessage;
        }

        public async Task SaveEmploymentCheckResult_Client(EmploymentCheckMessage employmentCheckMessage)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.SaveEmploymentCheckResult_Client()";

            try
            {
                if (employmentCheckMessage != null)
                {
                    await _employmentCheckService.SaveEmploymentCheckResult_Service(employmentCheckMessage);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: The employmentCheckMessage input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
