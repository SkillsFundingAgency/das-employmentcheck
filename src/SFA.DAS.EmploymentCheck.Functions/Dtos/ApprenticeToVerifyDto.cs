using System;

namespace SFA.DAS.EmploymentCheck.Functions.Dtos
{
    public class ApprenticeToVerifyDto
    {
        public ApprenticeToVerifyDto(string hashedAccountId, string nationalInsuranceNumber, long uln, long ukprn, DateTime actualStartDate)
        {
            HashedAccountId = hashedAccountId;
            NationalInsuranceNumber = nationalInsuranceNumber;
            ULN = uln;
            UKPRN = ukprn;
            ActualStartDate = actualStartDate;
        }

        public string HashedAccountId { get; }
        public string NationalInsuranceNumber { get; }
        public long ULN { get; }
        public long UKPRN { get; }
        public DateTime ActualStartDate { get; }
    }
}
