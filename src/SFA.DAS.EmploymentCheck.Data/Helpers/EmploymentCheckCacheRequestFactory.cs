using System;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.Helpers
{
    public class EmploymentCheckCacheRequestFactory
        : IEmploymentCheckCacheRequestFactory
    {
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
            if (nino.Length > 20) { nino = nino.Substring(0, 20); }
            employmentCheckCacheRequest.Nino = nino;
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
