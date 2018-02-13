using System;
using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.EmploymentCheck.Events
{
    [MessageGroup("employment_check_required")]
    public class EmploymentCheckRequiredForApprenticeMessage
    {
        public EmploymentCheckRequiredForApprenticeMessage() { }

        public EmploymentCheckRequiredForApprenticeMessage(string nationalInsuranceNumber, long uln, long apprenticeshipId, long ukprn, DateTime actualStartDate)
        {
            NationalInsuranceNumber = nationalInsuranceNumber;
            Uln = uln;
            ApprenticeshipId = apprenticeshipId;
            Ukprn = ukprn;
            ActualStartDate = actualStartDate;
        }

        public string NationalInsuranceNumber { get; set; }
        public long Uln { get; set; }
        public long ApprenticeshipId { get; set; }
        public long Ukprn { get; set;  }
        public DateTime ActualStartDate { get; set; }
    }
}
