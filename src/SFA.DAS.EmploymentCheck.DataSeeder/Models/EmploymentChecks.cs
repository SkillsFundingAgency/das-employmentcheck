using System;
using Dapper.Contrib.Extensions;

namespace SFA.DAS.EmploymentCheck.DataSeeder.Models
{
    [Table("Business.EmploymentCheck")]
    public class EmploymentChecks
    {
        [Key]
        public long Id { get; set; }
        public Guid CorrelationId { get; set; }
        public string CheckType { get; set; }
        public long ULN { get; set; }
        public long ApprenticeshipId { get; set; }
        public long AccountId { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public bool? Employed { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}