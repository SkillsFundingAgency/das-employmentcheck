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
            return await _employmentCheckRepository.GetEmploymentChecksBatch();
        }

        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{nameof(EmploymentCheckService)}.CreateEmploymentCheckCacheRequests";
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            // Validate that the employmentCheckData lists have data
            var employmentCheckDataValidator = new EmploymentCheckDataValidator();
            var employmentCheckDataValidatorResult = await employmentCheckDataValidator.ValidateAsync(employmentCheckData);

            // EmploymentCheckData validation - failed
            // Log the validation errors
            if (!employmentCheckDataValidatorResult.IsValid)
            {
                foreach (var error in employmentCheckDataValidatorResult.Errors)
                {
                    _logger.LogError($"{thisMethodName}: ERROR - EmploymentCheckData: {error.ErrorMessage}");
                }

                return await Task.FromResult(new List<EmploymentCheckCacheRequest>());  // caller should check for an empty list of EmploymentCheckCacheRequests
            }

            // EmploymentCheckData validation - succeeded
            // Create an EmploymentCheckCacheRequest for each unique combination of Uln, National Insurance Number, PayeScheme, MinDate and MaxDate
            // e.g. if an apprentices employer has 900 paye schemes then we need to create 900 messages for the given Uln, National Insurance Number, PayeScheme, MinDate and MaxDate
            var employmentCheckValidator = new EmploymentCheckValidator();
            IList<EmploymentCheckCacheRequest> employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
            foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
            {
                // Validate the employmentCheck fields are not empty
                var employmentCheckValidatorResult = await employmentCheckValidator.ValidateAsync(employmentCheck);

                // EmploymentCheck validation - failed
                // Log the validation errors
                if (!employmentCheckValidatorResult.IsValid)
                {
                    foreach (var error in employmentCheckValidatorResult.Errors)
                    {
                        _logger.LogError($"{thisMethodName}: ERROR - EmploymentCheck: {error.ErrorMessage}");
                    }
                }

                // EmploymentCheckData validation - succeeded
                // Lookup the National Insurance Number for this apprentice in the employment check data
                var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.FirstOrDefault(ninumber => ninumber.Uln == employmentCheck.Uln)?.NiNumber;
                if (string.IsNullOrEmpty(nationalInsuranceNumber))
                {
                    // No national insurance number found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (Nino not found).");

                    continue;
                }

                // Lookup the PayeSchemes for this apprentice in the employment check data
                var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.FirstOrDefault(ps => ps.EmployerAccountId == employmentCheck.AccountId);
                if (employerPayeSchemes == null)
                {
                    // No paye schemes found for this apprentice so we're not able to do an employment check and will need to skip this apprentice
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (PayeScheme not found).");

                    continue;
                }

                // Create the individual EmploymentCheckCacheRequest combinations for each paye scheme
                foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                {
                    if (string.IsNullOrEmpty(payeScheme))
                    {
                        // An empty paye scheme so we're not able to do an employment check and will need to skip this
                        _logger.LogError($"{thisMethodName}: An empty PAYE scheme was found for apprentice Uln: [{employmentCheck.Uln}] accountId [{employmentCheck.AccountId}].");
                        continue;
                    }

                    var employmentCheckCacheRequest = new EmploymentCheckCacheRequest();
                    employmentCheckCacheRequest.ApprenticeEmploymentCheckId = employmentCheck.Id;
                    employmentCheckCacheRequest.CorrelationId = employmentCheck.CorrelationId;
                    employmentCheckCacheRequest.Nino = nationalInsuranceNumber;
                    employmentCheckCacheRequest.PayeScheme = payeScheme;
                    employmentCheckCacheRequest.MinDate = employmentCheck.MinDate;
                    employmentCheckCacheRequest.MaxDate = employmentCheck.MaxDate;
                    employmentCheckCacheRequest.CreatedOn = DateTime.Now;
                    employmentCheckCacheRequest.LastUpdatedOn = DateTime.Now;

                    employmentCheckRequests.Add(employmentCheckCacheRequest);

                    // Store the individual EmploymentCheckCacheRequest combinations for each paye scheme
                    try
                    {
                        await _employmentCheckCashRequestRepository.Save(employmentCheckCacheRequest);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"EmploymentCheckService.CreateEmploymentCheckCacheRequests(): ERROR: the EmploymentCheckCashRequest repository Save() method threw an Exception during the storing of the EmploymentCheckCacheRequest [{e}]");
                    }
                }
            }

            return await Task.FromResult(employmentCheckRequests);
        }

        public async Task<IList<EmploymentCheckCacheRequest>> SetCacheRequestRelatedRequestsProcessingStatus(
            Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> employmentCheckCacheRequestAndStatusToSet
        )
        {
            return await _employmentCheckCashRequestRepository.SetRelatedRequestsCompletionStatus(employmentCheckCacheRequestAndStatusToSet);
        }


        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            return await _employmentCheckCashRequestRepository.GetNext();
        }

        public async Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            Guard.Against.Null(employmentCheckCacheRequest, nameof(employmentCheckCacheRequest));

            if (employmentCheckCacheRequest.RequestCompletionStatus == (short)ProcessingCompletionStatus.Started)
            {
                employmentCheckCacheRequest.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
            }

            await _employmentCheckCashRequestRepository.UpdateEmployedAndRequestStatusFields(employmentCheckCacheRequest);

            var employmentCheck = new Models.EmploymentCheck
            {
                Id = employmentCheckCacheRequest.ApprenticeEmploymentCheckId,
                Employed = employmentCheckCacheRequest.Employed,
                RequestCompletionStatus = employmentCheckCacheRequest.RequestCompletionStatus
            };
            await _employmentCheckRepository.UpdateEmployedAndRequestStatusFields(employmentCheck);
        }
    }
}