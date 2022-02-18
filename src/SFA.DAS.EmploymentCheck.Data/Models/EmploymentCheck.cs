using System;
using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    [Table("Business.EmploymentCheck")]
    public class EmploymentCheck
    {
        public EmploymentCheck() { }

        public EmploymentCheck(
            Guid correlationId, 
            string checkType, 
            long uln, 
            long? apprenticeshipId, 
            int apprenticeshipAccountId, 
            DateTime minDate, 
            DateTime maxDate)
        {
            CorrelationId = correlationId;
            CheckType = checkType;
            Uln = uln;
            ApprenticeshipId = apprenticeshipId;
            AccountId = apprenticeshipAccountId;
            MinDate = minDate;
            MaxDate = maxDate;
        }

        [Key]
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

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }
    }

}

