using System;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmploymentCheck.Domain.Common;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    [Table("Cache.EmploymentCheckCacheResponse")]
    public class EmploymentCheckCacheResponse
        : Entity
    {
        public EmploymentCheckCacheResponse() { }

        public EmploymentCheckCacheResponse(
            long? employmentCheckId,
            long employmentCheckCacheRequestId,
            Guid? correlationId,
            bool? employed,
            string foundOnPaye,
            bool processingComplete,
            int count,
            string httpResponse,
            short httpStatusCode)
        {
            EmploymentCheckId = employmentCheckId;
            EmploymentCheckCacheRequestId = employmentCheckCacheRequestId;
            CorrelationId = correlationId;
            Employed = employed;
            FoundOnPaye = foundOnPaye;
            ProcessingComplete = processingComplete;
            Count = count;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
        }

        public long? EmploymentCheckId { get; set; }

        public long? EmploymentCheckCacheRequestId { get; set; }

        public Guid? CorrelationId { get; set; }

        public bool? Employed { get; set; }

        public string FoundOnPaye { get; set; }

        public bool ProcessingComplete { get; set; }

        public int Count { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }
    }
}
