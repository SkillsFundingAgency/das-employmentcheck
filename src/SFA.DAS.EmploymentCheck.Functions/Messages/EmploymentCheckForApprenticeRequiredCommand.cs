using System;

namespace SFA.DAS.EmploymentCheck.Functions.Messages
{
    public class EmploymentCheckForApprenticeRequired
    {
        public long ULN { get; }
        public string NationalInsuranceNumber { get; }
        public DateTime ActualStartDate { get; }
        public string HashedAccountId { get; }
        public long UKPRN { get; }

        public EmploymentCheckForApprenticeRequired(long uln, string nationalInsuranceNumber, DateTime actualStartDate, string hashedAccountId, long ukprn)
        {
            ULN = uln;
            NationalInsuranceNumber = nationalInsuranceNumber;
            ActualStartDate = actualStartDate;
            HashedAccountId = hashedAccountId;
            UKPRN = ukprn;
        }
    }
}
