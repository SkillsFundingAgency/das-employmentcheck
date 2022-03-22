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

        public string ErrorType { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public void SetRequestCompletionStatus(ProcessingCompletionStatus status)
        {
            RequestCompletionStatus = (short)status;
        }

        public static EmploymentCheck CreateEmploymentCheck(EmploymentCheckCacheRequest request)
        {
            var employmentCheck = new EmploymentCheck();

            employmentCheck.Id = request.ApprenticeEmploymentCheckId;
            employmentCheck.CorrelationId = (Guid)request.CorrelationId;
            employmentCheck.Employed = request.Employed;
            employmentCheck.RequestCompletionStatus = request.RequestCompletionStatus;
            employmentCheck.ErrorType = request.Employed == null ? "HmrcFailure" : null;

            return employmentCheck;
        }
    }
}

