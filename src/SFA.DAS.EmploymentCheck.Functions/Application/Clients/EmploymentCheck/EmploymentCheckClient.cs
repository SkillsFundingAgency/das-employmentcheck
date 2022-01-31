using Ardalis.GuardClauses;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public class EmploymentCheckClient
        : IEmploymentCheckClient
    {
        private readonly IEmploymentCheckService _employmentCheckService;

        public EmploymentCheckClient(IEmploymentCheckService employmentCheckService)
        {
            _employmentCheckService = employmentCheckService;
        }

        public async Task<IList<Models.EmploymentCheck>> GetEmploymentChecksBatch()
        {
            return await _employmentCheckService.GetEmploymentChecksBatch();
        }

        public async Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(
            EmploymentCheckData employmentCheckData)
        {
            Guard.Against.Null(employmentCheckData, nameof(employmentCheckData));

            var employmentCheckRequests = await _employmentCheckService.CreateEmploymentCheckCacheRequests(employmentCheckData);

            return employmentCheckRequests;
        }

        public async Task<EmploymentCheckCacheRequest> GetEmploymentCheckCacheRequest()
        {
            var employmentCheckCacheRequest = await _employmentCheckService.GetEmploymentCheckCacheRequest();

            return employmentCheckCacheRequest;
        }

        public async Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            await _employmentCheckService.StoreEmploymentCheckResult(employmentCheckCacheRequest);
        }

        public async Task UpdateRequestCompletionStatusForRelatedEmploymentCheckCacheRequests(Models.EmploymentCheckCacheRequest request)
        {
            await _employmentCheckService.UpdateRequestCompletionStatusForRelatedEmploymentCheckCacheRequests(request);
        }
    }
}