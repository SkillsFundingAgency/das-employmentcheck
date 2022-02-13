using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService
        : IEmploymentCheckService
    {
        private readonly ILogger<IEmploymentCheckService> _logger;
        private readonly IEmploymentCheckRepository _employmentCheckRepository;
        private readonly IEmploymentCheckCacheRequestRepository _employmentCheckCashRequestRepository;

        public EmploymentCheckService(
            ILogger<IEmploymentCheckService> logger,
            IEmploymentCheckRepository employmentCheckRepository,
            IEmploymentCheckCacheRequestRepository employmentCheckCashRequestRepository
        )
        {
            _logger = logger;
            _employmentCheckRepository = employmentCheckRepository;
            _employmentCheckCashRequestRepository = employmentCheckCashRequestRepository;
        }

        public async Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            return await _employmentCheckRepository.GetEmploymentChecksBatch() ?? new List<Models.EmploymentCheck>();
        }

        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData,
            IEmploymentCheckCacheRequestFactory cacheRequestFactory
        )
        {
            var thisMethodName = $"{nameof(EmploymentCheckService)}.CreateEmploymentCheckCacheRequests";
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            var employmentCheckDataValidator = new EmploymentCheckDataValidator();
            var employmentCheckDataValidatorResult = await employmentCheckDataValidator.ValidateAsync(employmentCheckData);
            if (!employmentCheckDataValidatorResult.IsValid)
            {
                foreach (var error in employmentCheckDataValidatorResult.Errors)
                {
                    _logger.LogError($"{thisMethodName}: ERROR - EmploymentCheckData: {error.ErrorMessage}");
                }

                return await Task.FromResult(new List<EmploymentCheckCacheRequest>());  // caller should check for an empty list of EmploymentCheckCacheRequests
            }

            var employmentCheckValidator = new EmploymentCheckValidator();
            IList<EmploymentCheckCacheRequest> employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
            foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
            {
                var employmentCheckValidatorResult = await employmentCheckValidator.ValidateAsync(employmentCheck);
                if (!employmentCheckValidatorResult.IsValid)
                {
                    foreach (var error in employmentCheckValidatorResult.Errors)
                    {
                        _logger.LogError($"{thisMethodName}: ERROR - EmploymentCheck: {error.ErrorMessage}");
                    }
                }

                var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.FirstOrDefault(ninumber => ninumber.Uln == employmentCheck.Uln)?.NiNumber;
                if (string.IsNullOrEmpty(nationalInsuranceNumber))
                {
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (Nino not found).");

                    employmentCheck.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
                    await _employmentCheckRepository.Save(employmentCheck);
                    continue;
                }

                var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.FirstOrDefault(ps => ps.EmployerAccountId == employmentCheck.AccountId);
                if (employerPayeSchemes == null)
                {
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (PayeScheme not found).");

                    employmentCheck.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
                    await _employmentCheckRepository.Save(employmentCheck);
                    continue;
                }

                foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                {
                    if (string.IsNullOrEmpty(payeScheme))
                    {
                        _logger.LogError($"{thisMethodName}: An empty PAYE scheme was found for apprentice Uln: [{employmentCheck.Uln}] accountId [{employmentCheck.AccountId}].");
                        continue;
                    }

                    var employmentCheckCacheRequest = await cacheRequestFactory.CreateEmploymentCheckCacheRequest(employmentCheck, nationalInsuranceNumber, payeScheme);
                    employmentCheckRequests.Add(employmentCheckCacheRequest);
                    await _employmentCheckCashRequestRepository.Save(employmentCheckCacheRequest);
                }
            }

            return await Task.FromResult(employmentCheckRequests);
        }

        public async Task<IList<EmploymentCheckCacheRequest>> SetCacheRequestRelatedRequestsProcessingStatus(
            Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet
        )
        {
            return await _employmentCheckCashRequestRepository.SetRelatedRequestsCompletionStatus(employmentCheckCacheRequestAndStatusToSet) ?? new List<EmploymentCheckCacheRequest>();
        }

        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            return await _employmentCheckCashRequestRepository.GetNext();
        }

        public async Task<long> StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            if (employmentCheckCacheRequest.RequestCompletionStatus == (short)ProcessingCompletionStatus.Started)
            {
                employmentCheckCacheRequest.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
            }
            await _employmentCheckCashRequestRepository.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest);
            return await _employmentCheckRepository.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest);
        }
    }
}