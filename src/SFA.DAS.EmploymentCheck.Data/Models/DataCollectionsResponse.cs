using System;
using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    [Table("Cache.DataCollectionsResponse")]
    public class DataCollectionsResponse
    {
        public DataCollectionsResponse() { }

        public DataCollectionsResponse(
            long id,
            long? apprenticeEmploymentCheckId,
            Guid? correlationId,
            long uln,
            string niNumber,
            string httpResponse,
            short httpStatusCode
        )
        {
            Id = id;
            ApprenticeEmploymentCheckId = apprenticeEmploymentCheckId;
            CorrelationId = correlationId;
            Uln = uln;
            NiNumber = niNumber;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
            CreatedOn = DateTime.Now;
            LastUpdatedOn = null;
        }

        [Dapper.Contrib.Extensions.Key]
        public long Id { get; set; }

        public long? ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long Uln { get; set; }

        [StringLength(20)]
        public string NiNumber { get; set; }

        [StringLength(8000)]
        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public static DataCollectionsResponse CreateResponse(long employmentCheckId, Guid correlationId, long uln, string httpResponse, short statusCode)
        {
            return new DataCollectionsResponse
            {
                ApprenticeEmploymentCheckId = employmentCheckId,
                CorrelationId = correlationId,
                Uln = uln,
                HttpResponse = $"{httpResponse[Range.EndAt(Math.Min(8000, httpResponse.Length))]}",
                HttpStatusCode = statusCode,
                LastUpdatedOn = DateTime.Now
            };
        }

        public void SetNiNumber(string niNumber)
        {
            NiNumber = niNumber;
        }
    }
}
