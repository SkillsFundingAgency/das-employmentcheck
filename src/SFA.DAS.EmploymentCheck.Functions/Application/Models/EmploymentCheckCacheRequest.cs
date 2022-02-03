using Dapper.Contrib.Extensions;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    [Table("Cache.EmploymentCheckCacheRequest")]
    public class EmploymentCheckCacheRequest
    {
        public long Id { get; set; }

        public long ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public string Nino { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public short? RequestCompletionStatus { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public void SetEmployed(bool value)
        {
            Employed = value;
            RequestCompletionStatus = (short)ProcessingCompletionStatus.Completed;
        }
    }
}
