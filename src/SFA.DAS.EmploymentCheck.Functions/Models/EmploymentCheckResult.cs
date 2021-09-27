using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmploymentCheck.Functions.Models
{
    public class EmploymentCheckResult
    {
		public long AccountId { get; set; }
        public long ULN { get; set; }
	    public long ApprenticeshipId { get; set; }
        public long UKPRN { get; set; }
        public string NationalInsuranceNumber { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public string CheckType { get; set; }
        public bool Result { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
