using System;
using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    [Table("Cache.EmploymentCheckCacheResponse")]
    public class EmploymentCheckCacheResponse
    {
        public EmploymentCheckCacheResponse() { } // parameterless default constructor is required for dapper 😕

        public EmploymentCheckCacheResponse(
            long? apprenticeEmploymentCheckId,
            long employmentCheckCacheRequestId,
            Guid? correlationId,
            bool? employed,
            string foundOnPaye,
            bool processingComplete,
            int count,
            string httpResponse,
            short httpStatusCode
        )
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
            LastUpdatedOn = null;
            CreatedOn = DateTime.Now;
        }

        [Key] public long Id { get; set; }

        public long? ApprenticeEmploymentCheckId { get; set; }

        public long? EmploymentCheckCacheRequestId { get; set; }

        public Guid? CorrelationId { get; set; }

        public bool? Employed { get; set; }

        public string FoundOnPaye { get; set; }

        public bool ProcessingComplete { get; set; }

        public int Count { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        internal static EmploymentCheckCacheResponse CreateSuccessfulCheckResponse(long apprenticeEmploymentCheckId,
            long employmentCheckCacheRequestId, Guid? correlationId, bool employed, string empref)
        {
            return new EmploymentCheckCacheResponse(
                apprenticeEmploymentCheckId,
                employmentCheckCacheRequestId,
                correlationId,
                employed,
                empref,
                true,
                1,
                "OK",
                (short)System.Net.HttpStatusCode.OK);
        }

        public static EmploymentCheckCacheResponse CreateCompleteCheckErrorResponse(long apprenticeEmploymentCheckId,
            long employmentCheckCacheRequestId, Guid? correlationId, string response, short httpStatusCode)
        {
            return new EmploymentCheckCacheResponse(
                apprenticeEmploymentCheckId,
                employmentCheckCacheRequestId,
                correlationId,
                null,
                null,
                true,
                1,
                response,
                httpStatusCode);
        }
    }
}
