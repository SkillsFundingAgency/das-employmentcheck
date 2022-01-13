using System;
using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    [Table("Business.EmploymentCheck")]
    public class EmploymentCheck
    {
        public EmploymentCheck() { }

        public EmploymentCheck(
            long id,
            Guid? correlationId,
            string checkType,
            long uln,
            long? apprenticeshipId,
            long accountId,
            DateTime minDate,
            DateTime maxDate,
            bool? employed,
            short requestCompletionStatus,
            DateTime lastUpdatedOn,
            DateTime createdOn)
        {
            Id = id;
            CorrelationId = correlationId;
            CheckType = checkType;
            Uln = uln;
            ApprenticeshipId = apprenticeshipId;
            AccountId = accountId;
            MinDate = minDate;
            MaxDate = maxDate;
            Employed = employed;
            RequestCompletionStatus = requestCompletionStatus;
            LastUpdatedOn = lastUpdatedOn;
            CreatedOn = createdOn;
        }

        public long Id { get; set; }

        public Guid? CorrelationId { get; set; }

        public string CheckType { get; set; }

        public long Uln { get; set; }

        public long? ApprenticeshipId { get; set; }

        public long AccountId { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public short? RequestCompletionStatus { get; set; }
        public short VersionId { get; set; }

        public DateTime LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}

