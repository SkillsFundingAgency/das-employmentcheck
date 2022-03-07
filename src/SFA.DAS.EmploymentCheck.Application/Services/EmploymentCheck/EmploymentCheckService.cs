using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using Models = SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck
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

        public async Task<Data.Models.EmploymentCheck> GetEmploymentCheck()
        {
            return await _employmentCheckRepository.GetEmploymentCheck();
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

        public async Task StoreCompletedCheck(Models.EmploymentCheck employmentCheck)
        {
            if(employmentCheck.Id > 0)
            {
                employmentCheck.LastUpdatedOn = DateTime.Now;
                employmentCheck.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
                await _employmentCheckRepository.InsertOrUpdate(employmentCheck);
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
            CheckParametersAreNotNullOrEmpty(employmentCheckData);
            var employmentCheckRequests = await BuildEmploymentCheckCacheRequestModel(employmentCheckData);
            return await Task.FromResult(employmentCheckRequests);
        }

        private async Task<IList<EmploymentCheckCacheRequest>> BuildEmploymentCheckCacheRequestModel(EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{nameof(EmploymentCheckService)}.CreateEmploymentCheckCacheRequests";

            IList<EmploymentCheckCacheRequest> employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
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

            return employmentCheckRequests;
        }

        private void CheckParametersAreNotNullOrEmpty(EmploymentCheckData employmentCheckData)
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
        }
    }
}