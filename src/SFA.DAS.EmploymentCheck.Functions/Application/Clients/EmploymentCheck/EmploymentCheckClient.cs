using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
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
        public async Task<IList<EmploymentCheckModel>> GetEmploymentChecksBatch(long employmentCheckLastHighestBatchId)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.GetEmploymentChecksBatch()";

            IList<EmploymentCheckModel> employmentCheckModels = null;
            try
            {
                employmentCheckModels = await _employmentCheckService.GetEmploymentChecksBatch(employmentCheckLastHighestBatchId);

                if (employmentCheckModels == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckModels value returned from the GetEmploymentChecksBatch() service returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckModels;
        }

        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given list of employment checks
        /// </summary>
        /// <param name="employmentCheckModels">The list of employment checks that require a an employment check cache request.</param>
        /// <returns>Task<IList<EmploymentCheckCacheRequest>></returns>
        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(IList<EmploymentCheckModel> employmentCheckModels)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.CreateEmploymentCheckCacheRequests()";

            IList<EmploymentCheckCacheRequest> employmentCheckCacheRequests = null;
            try
            {
                employmentCheckCacheRequests = await _employmentCheckService.CreateEmploymentCheckCacheRequests(employmentCheckModels);

                if (employmentCheckCacheRequests == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentCheckCacheRequests value returned from the CreateEmploymentCheckCacheRequests() service returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckCacheRequests;
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
                    await _employmentCheckService.EnqueueEmploymentCheckMessages(employmentCheckData);
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
                employmentCheckMessage = await _employmentCheckService.DequeueEmploymentCheckMessage();

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
                    await _employmentCheckService.SaveEmploymentCheckResult(employmentCheckMessage);
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
