using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    [Table("HmrcApiResponse", Schema = "Cache")]

    public class HmrcApiResponse
        : ApiResponseEntity
    {
        public HmrcApiResponse() {}

        public HmrcApiResponse(
            long? apprenticeEmploymentCheckId,
            Guid? correlationId,
            long uln,
            string nino,
            string httpResponse,
            short httpStatusCode,
            DateTime createdOn,
            DateTime lastUpdatedOn)
        {
            ApprenticeEmploymentCheckId = apprenticeEmploymentCheckId;
            CorrelationId = correlationId;
            Uln = uln;
            NiNumber = nino;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
            CreatedOn = createdOn;
            LastUpdatedOn = lastUpdatedOn;
        }

        public long? ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long Uln { get; set; }

        public string NiNumber { get; set; }
    }
}
