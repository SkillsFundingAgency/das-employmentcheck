using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public class EmploymentCheckClient : IEmploymentCheckClient
    {
        private readonly Services.EmploymentCheck.IEmploymentCheckClient _employmentCheckService;

        public EmploymentCheckClient(
            ILogger<IEmploymentCheckClient> logger,
            Services.EmploymentCheck.IEmploymentCheckClient employmentCheckService)
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

        public async Task<EmploymentCheckCacheRequest> ProcessEmploymentCheckCacheRequest()
        {
            var employmentCheckCacheRequest = await _employmentCheckService.GetEmploymentCheckCacheRequest();

            return employmentCheckCacheRequest;
        }

        public async Task StoreEmploymentCheckResult(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            await _employmentCheckService.StoreEmploymentCheckResult(employmentCheckCacheRequest);
        }

        public async Task AbandonRelatedRequests(Models.EmploymentCheckCacheRequest request)
        {
            await _employmentCheckService.AbandonRelatedRequests(request);
        }
    }
}