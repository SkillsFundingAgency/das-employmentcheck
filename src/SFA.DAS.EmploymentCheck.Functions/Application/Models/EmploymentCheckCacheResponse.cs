using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class EmploymentCheckCacheResponse
    {
        public EmploymentCheckCacheResponse() { }

        public EmploymentCheckCacheResponse(
            long? apprenticeEmploymentCheckId,
            long employmentCheckCacheRequestId,
            Guid? correlationId,
            bool? employed,
            string foundOnPaye,
            bool processingComplete,
            int count,
            string httpResponse,
            short httpStatusCode)
        {
            ApprenticeEmploymentCheckId = apprenticeEmploymentCheckId;
            EmploymentCheckCacheRequestId = employmentCheckCacheRequestId;
            CorrelationId = correlationId;
            Employed = employed;
            FoundOnPaye = foundOnPaye;
            ProcessingComplete = processingComplete;
            Count = count;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
        }

        public long? ApprenticeEmploymentCheckId { get; set; }

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
