using System;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService : IEmploymentCheckService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<IEmploymentCheckService> _logger;
        private readonly IEmploymentCheckRepository _employmentCheckRepository;
        private readonly IEmploymentCheckCacheRequestRepository _employmentCheckCacheRequestRepository;

        public EmploymentCheckService(
            ILogger<IEmploymentCheckService> logger,
            IEmploymentCheckRepository employmentCheckRepository,
            IEmploymentCheckCacheRequestRepository employmentCheckCacheRequestRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _employmentCheckRepository = employmentCheckRepository;
            _employmentCheckCacheRequestRepository = employmentCheckCacheRequestRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            return await _employmentCheckRepository.GetEmploymentChecksBatch();
        }

        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            return await _employmentCheckCacheRequestRepository.GetEmploymentCheckCacheRequest();
        }

        public async Task StoreCompletedCheck(EmploymentCheckCacheRequest request, EmploymentCheckCacheResponse response)
        {
            try
            {
                request.LastUpdatedOn = DateTime.Now;

                await _unitOfWork.BeginAsync();
                await _unitOfWork.UpdateAsync(request);
                await _unitOfWork.InsertAsync(response);
                await _employmentCheckRepository.UpdateEmploymentCheckAsComplete(request, _unitOfWork);

                if (response.Employed == true)
                {
                    await _employmentCheckCacheRequestRepository.AbandonRelatedRequests(request, _unitOfWork);
                }

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task InsertEmploymentCheckCacheResponse(EmploymentCheckCacheResponse response)
        {
            await _unitOfWork.BeginAsync();
            await _unitOfWork.InsertAsync(response);
            await _unitOfWork.CommitAsync();
        }

        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{nameof(EmploymentCheckService)}.CreateEmploymentCheckCacheRequests";
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            var employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
            foreach (var employmentCheck in employmentCheckData.EmploymentChecks)
            {
                var nationalInsuranceNumber = employmentCheckData.ApprenticeNiNumbers.FirstOrDefault(ninumber => ninumber.Uln == employmentCheck.Uln)?.NiNumber;
                if (string.IsNullOrEmpty(nationalInsuranceNumber))
                {
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (Nino not found).");

                    employmentCheck.RequestCompletionStatus = (short)ProcessingCompletionStatus.ProcessingError_NinoNotFound;
                    await _employmentCheckRepository.InsertOrUpdate(employmentCheck);
                    continue;
                }

                var employerPayeSchemes = employmentCheckData.EmployerPayeSchemes.FirstOrDefault(ps => ps.EmployerAccountId == employmentCheck.AccountId);
                if (employerPayeSchemes == null)
                {
                    _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheck.Uln}] (PayeScheme not found).");

                    employmentCheck.RequestCompletionStatus = (short)ProcessingCompletionStatus.ProcessingError_PayeSchemeNotFound;
                    await _employmentCheckRepository.InsertOrUpdate(employmentCheck);
                    continue;
                }

                foreach (var payeScheme in employerPayeSchemes.PayeSchemes)
                {
                    if (string.IsNullOrEmpty(payeScheme))
                    {
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

                    employmentCheckRequests.Add(employmentCheckCacheRequest);

                    await _employmentCheckCacheRequestRepository.Save(employmentCheckCacheRequest);
                }
            }

            return await Task.FromResult(employmentCheckRequests);
        }

    }
}