using System;
using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;
using KeyAttribute = Dapper.Contrib.Extensions.KeyAttribute;

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

        [Key]
        public long Id { get; set; }

        public long? ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long Uln { get; set; }

        [StringLength(20)]
        public string NiNumber { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
    }
}
