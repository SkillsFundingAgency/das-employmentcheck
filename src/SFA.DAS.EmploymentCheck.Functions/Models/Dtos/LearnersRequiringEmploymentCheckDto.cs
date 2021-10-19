using System;

namespace SFA.DAS.EmploymentCheck.Functions.Models.Dtos
{
    public class LearnersRequiringEmploymentCheckDto
    {
        public LearnersRequiringEmploymentCheckDto() { }

        public LearnersRequiringEmploymentCheckDto(
            long uln,
            long accountId,
            DateTime learnerStartDate,
            DateTime employmentCheckEffectiveDate,
            DateTime employmentCheckInEffectiveDate)
        {
            ULN = uln;
            AccountId = accountId;
            LearnerStartDate = learnerStartDate;
            EmploymentCheckEffectiveDate = employmentCheckEffectiveDate;
            EmploymentCheckInEffectiveDate = employmentCheckInEffectiveDate;
        }

        public long ULN { get; set; }
        public long AccountId { get; set; }

        public DateTime LearnerStartDate { get; set; }

        public DateTime EmploymentCheckEffectiveDate { get; set; }

        public DateTime EmploymentCheckInEffectiveDate { get; set; }
    }
}
