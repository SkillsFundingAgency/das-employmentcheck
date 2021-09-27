using System;

namespace SFA.DAS.EmploymentCheck.Functions.Dtos
{
    public class ApprenticeToVerifyDto
    {
        public ApprenticeToVerifyDto(long accountId, string nationalInsuranceNumber, long uln, long ukprn, long apprenticeshipId, DateTime startDate, DateTime endDate)
        {
            AccountId = accountId;
            NationalInsuranceNumber = nationalInsuranceNumber;
            ULN = uln;
            UKPRN = ukprn;
            ApprenticeshipId = apprenticeshipId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public long AccountId { get; }
        public string NationalInsuranceNumber { get; }
        public long ULN { get; }
        public long UKPRN { get; }
        public long ApprenticeshipId { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
    }
}
