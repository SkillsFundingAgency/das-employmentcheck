using System;
using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;
using KeyAttribute = Dapper.Contrib.Extensions.KeyAttribute;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    [Table("Cache.EmploymentCheckCacheRequest")]
    public class EmploymentCheckCacheRequest
    {
        public EmploymentCheckCacheRequest() { }

        [Key]
        public long Id { get; set; }

        public long ApprenticeEmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        [StringLength(20)] // Column size in db is 20, Autofixture generates string longer than 20 causing an exception in the test
        public string Nino { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public short? RequestCompletionStatus { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
