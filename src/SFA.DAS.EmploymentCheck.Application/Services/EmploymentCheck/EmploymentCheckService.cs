using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService : IEmploymentCheckService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmploymentCheckRepository _employmentCheckRepository;
        private readonly IEmploymentCheckCacheRequestRepository _employmentCheckCacheRequestRepository;

        public EmploymentCheckService(
            IEmploymentCheckRepository employmentCheckRepository,
            IEmploymentCheckCacheRequestRepository employmentCheckCacheRequestRepository,
            IUnitOfWork unitOfWork)
        {
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

        public async Task StoreCompletedEmploymentCheck(Models.EmploymentCheck employmentCheck)
        {
            employmentCheck.LastUpdatedOn = DateTime.Now;
            employmentCheck.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
            await _employmentCheckRepository.InsertOrUpdate(employmentCheck);
        }

        public async Task StoreCompletedCheck(EmploymentCheckCacheRequest request, EmploymentCheckCacheResponse response)
        {
            try
            {
                request.LastUpdatedOn = DateTime.Now;

                await _unitOfWork.BeginAsync();
                await _unitOfWork.UpdateAsync(request);
                await _unitOfWork.InsertAsync(response);

                var employmentCheck = CreateEmploymentCheck(request);
                await _employmentCheckRepository.UpdateEmploymentCheckAsComplete(employmentCheck, _unitOfWork);

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

        public async Task SaveEmploymentCheck(Data.Models.EmploymentCheck check)
        {
           await _employmentCheckRepository.InsertOrUpdate(check);
        }

        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(EmploymentCheckData employmentCheckData)
        {
            var employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
            foreach (var payeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes)
            {
                var employmentCheckCacheRequest = new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = employmentCheckData.EmploymentCheck.Id,
                    CorrelationId = employmentCheckData.EmploymentCheck.CorrelationId,
                    Nino = employmentCheckData.ApprenticeNiNumber.NiNumber,
                    PayeScheme = payeScheme,
                    MinDate = employmentCheckData.EmploymentCheck.MinDate,
                    MaxDate = employmentCheckData.EmploymentCheck.MaxDate
                };
                employmentCheckRequests.Add(employmentCheckCacheRequest);

                await _employmentCheckCacheRequestRepository.Save(employmentCheckCacheRequest);
            }

            return employmentCheckRequests;
        }

        public Data.Models.EmploymentCheck CreateEmploymentCheck(EmploymentCheckCacheRequest request)
        {
            var employmentCheck = new Data.Models.EmploymentCheck();

            employmentCheck.Id = request.ApprenticeEmploymentCheckId;
            employmentCheck.Employed = request.Employed;
            employmentCheck.RequestCompletionStatus = request.RequestCompletionStatus;
            employmentCheck.ErrorType = request.Employed == null ? "HmrcFailure" : null;

            return employmentCheck;
        }
    }
}