using Dapper.Contrib.Extensions;
using System;

namespace app_levy_data_seeder.Models
{
    [Table("ApprenticeEmploymentCheckMessageQueue")]
    public class ApprenticeEmploymentCheckMessageQueue
    {
        public Guid MessageId { get; set; }
        public DateTime MessageCreatedDateTime { get; set; }
        public long EmploymentCheckId { get; set; }
        public long Uln { get; set; }
        public string NationalInsuranceNumber { get; set; }
        public string PayeScheme { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}
