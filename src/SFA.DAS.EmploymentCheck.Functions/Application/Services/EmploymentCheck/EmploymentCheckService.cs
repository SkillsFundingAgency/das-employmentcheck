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

        public async Task<IList<Models.EmploymentCheck>> GetEmploymentCheck()
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

            if (string.IsNullOrEmpty(employmentCheckData.ApprenticeNiNumber.NiNumber))
            {
                _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheckData.EmploymentCheck.Uln}] (Nino not found).");
                throw new ArgumentException("Nino not found");
            }
   
            if (employmentCheckData.EmployerPayeSchemes == null || employmentCheckData.EmployerPayeSchemes.PayeSchemes == null)
            {
                _logger.LogError($"{thisMethodName}: ERROR - Unable to create an EmploymentCheckCacheRequest for apprentice Uln: [{employmentCheckData.EmploymentCheck.Uln}] (PayeScheme not found).");
                throw new ArgumentException("Paye scheme not found");
            }

            foreach (var payeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes)
            {
                if (string.IsNullOrEmpty(payeScheme))
                {
                    _logger.LogError($"{thisMethodName}: An empty PAYE scheme was found for apprentice Uln: [{employmentCheckData.EmploymentCheck.Uln}] accountId [{employmentCheckData.EmploymentCheck.AccountId}].");
                    continue;
                }

                var employmentCheckCacheRequest = new EmploymentCheckCacheRequest();
                employmentCheckCacheRequest.ApprenticeEmploymentCheckId = employmentCheckData.EmploymentCheck.Id;
                employmentCheckCacheRequest.CorrelationId = employmentCheckData.EmploymentCheck.CorrelationId;
                employmentCheckCacheRequest.Nino = employmentCheckData.ApprenticeNiNumber.NiNumber;
                employmentCheckCacheRequest.PayeScheme = payeScheme;
                employmentCheckCacheRequest.MinDate = employmentCheckData.EmploymentCheck.MinDate;
                employmentCheckCacheRequest.MaxDate = employmentCheckData.EmploymentCheck.MaxDate;

                employmentCheckRequests.Add(employmentCheckCacheRequest);

                    await _employmentCheckCacheRequestRepository.Save(employmentCheckCacheRequest);
                }
            }

            return await Task.FromResult(employmentCheckRequests);
        }

    }
}