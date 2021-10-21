using System;

namespace SFA.DAS.EmploymentCheck.Functions.Models.Dtos
{
    public class ApprenticeToVerifyDto
    {
        public ApprenticeToVerifyDto() { }

        public ApprenticeToVerifyDto(long id, long accountId, string nationalInsuranceNumber, long uln, long ukprn, long apprenticeshipId, DateTime startDate, DateTime endDate)
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

        public static implicit operator ApprenticeToVerifyDto(LearnerRequiringEmploymentCheckDto source)
        {
            return new ApprenticeToVerifyDto
            {
                AccountId = source.AccountId,
                ApprenticeshipId = source.ApprenticeshipId,
                EndDate = source.EmploymentCheckInEffectiveDate,
                Id = source.Id,
                NationalInsuranceNumber = source.NationalInsuranceNumber,
                StartDate = source.EmploymentCheckEffectiveDate,
                UKPRN = source.UKPRN,
                ULN = source.ULN
            };
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
