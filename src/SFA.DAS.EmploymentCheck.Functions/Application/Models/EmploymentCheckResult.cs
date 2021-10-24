using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class EmploymentCheckResult
    {
        public long Id { get; set; }
		public long AccountId { get; set; }
        public long ULN { get; set; }
	    public long ApprenticeshipId { get; set; }
        public long UKPRN { get; set; }
        public string NationalInsuranceNumber { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public string CheckType { get; set; }
        public bool IsEmployed { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool HasBeenChecked { get; set; }
    }
}
