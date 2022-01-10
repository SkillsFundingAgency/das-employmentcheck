using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Interfaces.PaymentsCompliance;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Domain.Common.Dtos;

namespace SFA.DAS.EmploymentCheck.Application.Clients.PaymentsCompliance
{
    public class PaymentsComplianceClient
        : IPaymentsComplianceClient
    {
        #region Private members
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly ILogger<IPaymentsComplianceClient> _logger;
        private readonly IPaymentsComplianceService _employmentCheckService;
        #endregion Private members

        #region Constructors
        public PaymentsComplianceClient(
            ILogger<IPaymentsComplianceClient> logger,
            IPaymentsComplianceService employmentCheckService)
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
        public async Task<IList<Domain.Entities.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            IList<Domain.Entities.EmploymentCheck> employmentChecks = new List<Domain.Entities.EmploymentCheck>();
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

        #region GetEmploymentCheckCacheRequest
        public async Task<EmploymentCheckCacheRequest> GetNextEmploymentCheckCacheRequest()
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            EmploymentCheckCacheRequest employmentCheckCacheRequest = null;
            try
            {
                employmentCheckCacheRequest = await _employmentCheckService.GetNextEmploymentCheckCacheRequest();

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
        #endregion GetEmploymentCheckCacheRequest

        #region StoreEmploymentCheckResult
        public async Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            var thisMethodName = $"\n\n{nameof(PaymentsComplianceClient)}.StoreEmploymentCheckCachRequest()";

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
        #endregion StoreEmploymentCheckResult
    }
}