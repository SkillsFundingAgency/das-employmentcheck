using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class Apprentice
    {
        public Apprentice() { }

        public Apprentice(long id, long accountId, string nationalInsuranceNumber, long uln, long ukprn, long apprenticeshipId, DateTime startDate, DateTime endDate)
        {
            Id = id;
            AccountId = accountId;
            NationalInsuranceNumber = nationalInsuranceNumber;
            ULN = uln;
            UKPRN = ukprn;
            ApprenticeshipId = apprenticeshipId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public long Id { get; set; }

        public long AccountId { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public long ULN { get; set; }

        public long UKPRN { get; set; }

        public long ApprenticeshipId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
