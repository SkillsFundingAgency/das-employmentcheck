using Dapper.Contrib.Extensions;
using System;

namespace app_levy_data_seeder.Models
{
    [Table("EmploymentCheckMessageQueue")]
    public class EmploymentCheckMessageQueue
    {

        public long Id { get; set; }

        public long EmploymentCheckId { get; set; }

        public long CorrelationId { get; set; }

        public long Uln { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDateTime { get; set; }

        public DateTime MaxDateTime { get; set; }

        public bool? Employed { get; set; }

        public DateTime? LastEmploymentCheck { get; set; }

        public string ResponseId { get; set; }

        public string ResponseMessage { get; set; }
    }
}
