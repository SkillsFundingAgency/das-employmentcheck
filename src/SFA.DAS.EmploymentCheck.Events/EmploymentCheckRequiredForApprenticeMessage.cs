using System;
using SFA.DAS.Messaging.Attributes;

namespace SFA.DAS.EmploymentCheck.Events
{
    [MessageGroup("employment_check_required")]
    public class EmploymentCheckRequiredForApprenticeMessage
    {
        public EmploymentCheckRequiredForApprenticeMessage() { }

        public EmploymentCheckRequiredForApprenticeMessage(string nationalInsuranceNumber, long uln, long? employerAccountId, long ukprn, DateTime? actualStartDate)
        {
            NationalInsuranceNumber = nationalInsuranceNumber;
            Uln = uln;
            EmployerAccountId = employerAccountId;
            Ukprn = ukprn;
            ActualStartDate = actualStartDate;
        }

        public string NationalInsuranceNumber { get; set; }
        public long Uln { get; set; }
        public long? EmployerAccountId { get; set; }
        public long Ukprn { get; }
        public DateTime? ActualStartDate { get; set; }
    }
}
