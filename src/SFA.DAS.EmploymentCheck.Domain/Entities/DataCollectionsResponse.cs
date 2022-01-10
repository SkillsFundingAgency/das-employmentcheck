using System;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmploymentCheck.Domain.Common;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    [Table("Cache.DataCollectionsResponse")]
    public class DataCollectionsResponse
        : Entity
    {
        public DataCollectionsResponse() { }

        public DataCollectionsResponse(
            long? employmentCheckId,
            Guid? correlationId,
            long uln,
            string niNumber,
            string httpResponse,
            short httpStatusCode)
        {
            EmploymentCheckId = employmentCheckId;
            CorrelationId = correlationId;
            Uln = uln;
            NiNumber = niNumber;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
        }

        public long? EmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long Uln { get; set; }

        public string NiNumber { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }
    }
}
