using System;
using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    [Table("Cache.EmploymentCheckCacheRequest")]
    public class EmploymentCheckCacheRequest
    {
        public EmploymentCheckCacheRequest() { }

        public EmploymentCheckCacheRequest(
            long id,
            long apprenticeshipId,
            Guid correlationId,
            string nino,
            string payeScheme,
            DateTime minDate,
            DateTime maxDate,
            bool? employed,
            short requestCompletionStatus,
            DateTime lastUpdatedOn,
            DateTime createdOn
        )
        {
            Id = id;
            ApprenticeEmploymentCheckId = apprenticeshipId;
            CorrelationId = correlationId;
            Nino = nino;
            PayeScheme = payeScheme;
            MinDate = minDate;
            MaxDate = maxDate;
            Employed = employed;
            RequestCompletionStatus = requestCompletionStatus;
            LastUpdatedOn = lastUpdatedOn;
            CreatedOn = DateTime.Now;
        }
    public long Id { get; set; }

        public long ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public string Nino { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public short? RequestCompletionStatus { get; set; }

        public DateTime LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
