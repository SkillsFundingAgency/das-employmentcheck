using System;

namespace SFA.DAS.EmploymentCheck.Events
{
    public class EmploymentCheckCompleteEvent
    {
        public EmploymentCheckCompleteEvent() { }

        public EmploymentCheckCompleteEvent(string nationalInsuranceNumber, long uln, long employerAccountId, long ukprn, DateTime checkDate, bool checkPassed)
        {
            NationalInsuranceNumber = nationalInsuranceNumber;
            Uln = uln;
            EmployerAccountId = employerAccountId;
            Ukprn = ukprn;
            CheckDate = checkDate;
            CheckPassed = checkPassed;
        }

        public string NationalInsuranceNumber { get; set; }
        public long Uln { get; set; }
        public long EmployerAccountId { get; set; }
        public long Ukprn { get; set; }
        public DateTime CheckDate { get; set; }
        public bool CheckPassed { get; set; }
    }
}
