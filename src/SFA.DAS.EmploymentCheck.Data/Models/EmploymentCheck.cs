using System;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmploymentCheck.Domain.Enums;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    [Table("Business.EmploymentCheck")]
    public class EmploymentCheck
    {
        public EmploymentCheck() { }

        [Key]
        public long Id { get; set; }

        public Guid CorrelationId { get; set; }

        public string CheckType { get; set; }

        public long Uln { get; set; }

        public long? ApprenticeshipId { get; set; }

        public long AccountId { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public short? RequestCompletionStatus { get; set; }

        private string _errorType;
        public string ErrorType
        {
            get { return Employed == null ? "HmrcFailure" : _errorType; }
            set { _errorType = value; }
        }

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public void SetRequestCompletionStatus(ProcessingCompletionStatus status)
        {
            RequestCompletionStatus = (short)status;
        }
    }
}

