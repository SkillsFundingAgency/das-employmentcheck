using System;

namespace SFA.DAS.EmploymentCheck.Functions.Models.Dtos
{
    public class LearnerRequiringEmploymentCheckDto
    {
        public LearnerRequiringEmploymentCheckDto() { }

        public LearnerRequiringEmploymentCheckDto(
            long id,
            long uln,
            long accountId,
            string nationalInsuranceNumber,
            long ukprn,
            long apprenticeshipId,
            DateTime employmentCheckEffectiveDate,
            DateTime employmentCheckInEffectiveDate)
        {
            Id = id;
            ULN = uln;
            AccountId = accountId;
            NationalInsuranceNumber = nationalInsuranceNumber;
            UKPRN = ukprn;
            ApprenticeshipId = apprenticeshipId;
            EmploymentCheckEffectiveDate = employmentCheckEffectiveDate;
            EmploymentCheckInEffectiveDate = employmentCheckInEffectiveDate;
        }

        public long Id { get; set; }

        public long ULN { get; set; }

        public long AccountId { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public long UKPRN { get; set; }

        public long ApprenticeshipId { get; set; }

        public DateTime EmploymentCheckEffectiveDate { get; set; }

        public DateTime EmploymentCheckInEffectiveDate { get; set; }
    }
}
