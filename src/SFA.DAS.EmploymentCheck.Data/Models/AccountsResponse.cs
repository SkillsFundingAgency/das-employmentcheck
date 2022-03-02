using System;
using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    [Table("Cache.AccountsResponse")]
    public class AccountsResponse
    {
        public AccountsResponse() { }

        public AccountsResponse(
            long id,
            long? apprenticeEmploymentCheckId,
            Guid? correlationId,
            long accountId,
            string payeSchemes,
            string httpResponse,
            short httpStatusCode,
            DateTime? lastUpdatedOn
        )
        {
            Id = id;
            ApprenticeEmploymentCheckId = apprenticeEmploymentCheckId;
            CorrelationId = correlationId;
            AccountId = accountId;
            PayeSchemes = payeSchemes;
            HttpResponse = httpResponse;
            HttpStatusCode = httpStatusCode;
            CreatedOn = DateTime.Now;
            LastUpdatedOn = lastUpdatedOn;
        }

        [Key]
        public long Id { get; set; }

        public long? ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public long AccountId { get; set; }

        public string PayeSchemes { get; set; }

        public string HttpResponse { get; set; }

        public short HttpStatusCode { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
    }
}
