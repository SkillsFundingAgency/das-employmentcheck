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

        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            return await _employmentCheckCashRequestRepository.GetEmploymentCheckCacheRequest();
        }

        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{nameof(EmploymentCheckService)}.CreateEmploymentCheckCacheRequests";
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            IList<EmploymentCheckCacheRequest> employmentCheckRequests = new List<EmploymentCheckCacheRequest>();
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

                    await _employmentCheckCashRequestRepository.Save(employmentCheckCacheRequest);
                }
            }

            return await Task.FromResult(employmentCheckRequests);
        }

    }
}