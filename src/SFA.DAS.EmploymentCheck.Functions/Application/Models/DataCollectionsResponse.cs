using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class DataCollectionsResponse
    {
        public DataCollectionsResponse() { }

        public DataCollectionsResponse(
            long? apprenticeEmploymentCheckId,
            Guid? correlationId,
            long uln,
            string niNumber,
            string httpResponse,
            short httpStatusCode)
        {
            ApprenticeEmploymentCheckId = apprenticeEmploymentCheckId;
            CorrelationId = correlationId;
            Uln = uln;
            NiNumber = niNumber;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
        }

        public long? ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long Uln { get; set; }

        public string NiNumber { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }
    }
}
