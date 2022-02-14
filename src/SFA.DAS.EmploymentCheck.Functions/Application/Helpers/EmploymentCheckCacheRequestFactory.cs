using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Helpers
{
    public class EmploymentCheckCacheRequestFactory
        : IEmploymentCheckCacheRequestFactory
    {
        // Quick and dirty solution for mocking the EmploymentCheckService.CreateEmploymentCheckRequest() method
        // which 'newed' up an EmploymentCheckCacheRequest that wasn't mockable and messed up the tests

        public EmploymentCheckCacheRequestFactory() {}

        public async Task<EmploymentCheckCacheRequest> CreateEmploymentCheckCacheRequest(
            Models.EmploymentCheck employmentCheck,
            string nino,
            string payeScheme)
        {
            var employmentCheckCacheRequest = new EmploymentCheckCacheRequest();

            employmentCheckCacheRequest.Id = -1;
            employmentCheckCacheRequest.ApprenticeEmploymentCheckId = employmentCheck.Id;
            employmentCheckCacheRequest.CorrelationId = employmentCheck.CorrelationId;
            employmentCheckCacheRequest.Nino = nino;
            if (payeScheme.Length > 20) { payeScheme = payeScheme.Substring(0, 20); }
            employmentCheckCacheRequest.PayeScheme = payeScheme;
            employmentCheckCacheRequest.MinDate = employmentCheck.MinDate;
            employmentCheckCacheRequest.MaxDate = employmentCheck.MaxDate;
            if (employmentCheck.Employed.HasValue) { employmentCheckCacheRequest.Employed = employmentCheck.Employed; }
            employmentCheckCacheRequest.RequestCompletionStatus = employmentCheck.RequestCompletionStatus;
            employmentCheckCacheRequest.CreatedOn = DateTime.Now;

            return await Task.FromResult(employmentCheckCacheRequest);
        }
    }
}
