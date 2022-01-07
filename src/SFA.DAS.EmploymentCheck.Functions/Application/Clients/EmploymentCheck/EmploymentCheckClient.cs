using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public class EmploymentCheckClient
        : IEmploymentCheckClient
    {
        #region Private members
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly ILogger<IEmploymentCheckClient> _logger;
        private readonly IEmploymentCheckService _employmentCheckService;
        #endregion Private members

        #region Constructors
        public EmploymentCheckClient(
            ILogger<IEmploymentCheckClient> logger,
            IEmploymentCheckService employmentCheckService)
        {
            _logger = logger;
            _employmentCheckService = employmentCheckService;
        }
        #endregion Constructors

        #region GetEmploymentChecksBatch
        /// <summary>
        /// Get a batch of the employment checks from the Employment Check database
        /// </summary>
        /// <returns>Task<IList<ApprenticeEmploymentCheck>></returns>
        public async Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            IList<Models.EmploymentCheck> employmentChecks = new List<Models.EmploymentCheck>();
            try
            {
                employmentChecks = await _employmentCheckService.GetEmploymentChecksBatch();

                if (employmentChecks == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The employmentChecks value returned from the GetEmploymentCheckBatch() service returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentChecks;
        }
        #endregion GetEmploymentChecksBatch

        #region CreateEmploymentCheckCacheRequests
        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given list of employment checks
        /// </summary>
        /// <param name="employmentCheckData">The list of data containing the employment checks that require a an employment check cache request.</param>
        /// <returns>Task<IList<EmploymentCheckCacheRequest>></returns>
        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            IList<EmploymentCheckCacheRequest> employmentCheckRequests = null;
            try
            {
                employmentCheckRequests = await _employmentCheckService.CreateEmploymentCheckCacheRequests(employmentCheckData);

                if (employmentCheckRequests == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The  value returned from CreateEmploymentCheckCacheRequests() returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckRequests;
        }
        #endregion CreateEmploymentCheckCacheRequests

        #region ProcessEmploymentCheckCacheRequest
        public async Task<EmploymentCheckCacheRequest> ProcessEmploymentCheckCacheRequest()
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            EmploymentCheckCacheRequest employmentCheckCacheRequest = null;
            try
            {
                employmentCheckCacheRequest = await _employmentCheckService.GetEmploymentCheckCacheRequest();

                if (employmentCheckCacheRequest == null)
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The  value returned from ProcessEmploymentCheckCacheRequest() returned null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}.{ex.StackTrace}");
            }

            return employmentCheckCacheRequest;
        }
        #endregion ProcessEmploymentCheckCacheRequest

        #region StoreEmploymentCheckCacheRequest

        public async Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"\n\n{nameof(EmploymentCheckClient)}.StoreEmploymentCheckCachRequest()";

            try
            {
                if (employmentCheckCacheRequest != null)
                {
                    await _employmentCheckService.StoreEmploymentCheckResult(employmentCheckCacheRequest);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: The employmentCheckCacheRequest input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
        #endregion StoreEmploymentCheckCacheRequest
    }
}