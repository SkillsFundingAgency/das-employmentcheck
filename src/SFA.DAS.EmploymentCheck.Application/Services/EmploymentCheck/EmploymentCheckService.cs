using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck
{
    public class EmploymentCheckService : IEmploymentCheckService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmploymentCheckService> _logger;
        private readonly IEmploymentCheckRepository _employmentCheckRepository;
        private readonly IEmploymentCheckCacheRequestRepository _employmentCheckCacheRequestRepository;

        public EmploymentCheckService(
            IEmploymentCheckRepository employmentCheckRepository,
            IEmploymentCheckCacheRequestRepository employmentCheckCacheRequestRepository,
            IUnitOfWork unitOfWork,
            ILogger<EmploymentCheckService> logger)
        {
            _employmentCheckRepository = employmentCheckRepository;
            _employmentCheckCacheRequestRepository = employmentCheckCacheRequestRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Data.Models.EmploymentCheck> GetEmploymentCheck()
        {
            return await _employmentCheckRepository.GetEmploymentCheck();
        }

        public async Task<Data.Models.EmploymentCheck> GetResponseEmploymentCheck()
        {
            return await _employmentCheckRepository.GetResponseEmploymentCheck();
        }

        public async Task<EmploymentCheckCacheRequest[]> GetEmploymentCheckCacheRequests()
        {
            return await _employmentCheckCacheRequestRepository.GetEmploymentCheckCacheRequests();
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
                request.RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;

                await _unitOfWork.BeginAsync();
                await _unitOfWork.UpdateAsync(request);
                await _unitOfWork.InsertAsync(response);

                var employmentCheck = Models.EmploymentCheck.CreateEmploymentCheck(request);

                await _employmentCheckRepository.UpdateEmploymentCheckAsComplete(employmentCheck, _unitOfWork);

                await _unitOfWork.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(EmploymentCheckService)}: CorrelationId: {request.CorrelationId}, EmploymentCheckRequestId: {request.Id} Error in Store Completed Check [{e}]");

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
            var learnerPayeCheckPriorities = await GetLearnerPayeCheckPriority(employmentCheckData);

            var employmentCheckRequests = new List<EmploymentCheckCacheRequest>();

            foreach (var prioritisedPayeScheme in learnerPayeCheckPriorities)
            {
                var employmentCheckCacheRequest = new EmploymentCheckCacheRequest
                {
                    ApprenticeEmploymentCheckId = employmentCheckData.EmploymentCheck.Id,
                    CorrelationId = employmentCheckData.EmploymentCheck.CorrelationId,
                    Nino = employmentCheckData.ApprenticeNiNumber.NiNumber,
                    PayeScheme = prioritisedPayeScheme.PayeScheme,
                    PayeSchemePriority = prioritisedPayeScheme.PriorityOrder,
                    MinDate = employmentCheckData.EmploymentCheck.MinDate,
                    MaxDate = employmentCheckData.EmploymentCheck.MaxDate
                };

                employmentCheckRequests.Add(employmentCheckCacheRequest);

                await _employmentCheckCacheRequestRepository.Save(employmentCheckCacheRequest);
            }

            return employmentCheckRequests;
        }

        public async Task AbandonRelatedRequests(EmploymentCheckCacheRequest[] employmentCheckCacheRequests)
        {
            foreach (var request in employmentCheckCacheRequests.Where(r => r.Employed == true))
            {
                try
                {
                    await _unitOfWork.BeginAsync();

                    await _employmentCheckCacheRequestRepository.AbandonRelatedRequests(request, _unitOfWork);

                    await _unitOfWork.CommitAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError($"{nameof(EmploymentCheckService)}: CorrelationId: {request.CorrelationId} EmploymentCheckRequestId: {request.Id}  Error in Abandon Related Requests [{e}]");

                    await _unitOfWork.RollbackAsync();

                    throw;
                }
            }
        }

        private async Task<List<LearnerPayeCheckPriority>> GetLearnerPayeCheckPriority(EmploymentCheckData employmentCheckData)
        {
            List<LearnerPayeCheckPriority> learnerPayeCheckPriorities;

            if (employmentCheckData.EmployerPayeSchemes.PayeSchemes.Count == 1 )
            {
                learnerPayeCheckPriorities = new List<LearnerPayeCheckPriority> { new LearnerPayeCheckPriority(employmentCheckData.EmployerPayeSchemes.PayeSchemes.First(), 1) };
            }
            else
            {
                //Get Priority from DB or initialise empty list
                learnerPayeCheckPriorities = await _employmentCheckCacheRequestRepository.GetLearnerPayeCheckPriority(employmentCheckData.ApprenticeNiNumber.NiNumber) 
                                             ?? new List<LearnerPayeCheckPriority>();

                if (learnerPayeCheckPriorities.Any())
                {
                    //remove any that doesn't exists in the current collection
                    var removed = learnerPayeCheckPriorities.RemoveAll(priority =>
                        !employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any(scheme =>
                            string.Equals(priority.PayeScheme, scheme, StringComparison.OrdinalIgnoreCase)));

                    //if any removed re-order the list
                    if (removed > 0 && learnerPayeCheckPriorities.Any())
                    {
                        for (var index = 0; index < learnerPayeCheckPriorities.Count; index++)
                        {
                            var learnerPayeCheckPriority = learnerPayeCheckPriorities[index];
                            learnerPayeCheckPriority.PriorityOrder = index + 1;
                        }
                    }
                }

                //add remaining items from current collection after the ordered list 
                var order = learnerPayeCheckPriorities.Count + 1;

                foreach (var payeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes)
                {
                    if (learnerPayeCheckPriorities.Any(lp => 
                            string.Equals(lp.PayeScheme, payeScheme, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    learnerPayeCheckPriorities.Add(new LearnerPayeCheckPriority(payeScheme, order++));
                }
            }

            return learnerPayeCheckPriorities;
        }
    }
}