using Dapper.Contrib.Extensions;
using System;

namespace app_levy_data_seeder.Models
{
    [Table("EmploymentCheck")]
    public class EmploymentCheck
    {
        public long Id { get; set; }

        public long CorrelationId { get; set; }

        public string CheckType { get; set; }

        public long Uln { get; set; }

        public long ApprenticeshipId { get; set; }

        public long AccountId { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
