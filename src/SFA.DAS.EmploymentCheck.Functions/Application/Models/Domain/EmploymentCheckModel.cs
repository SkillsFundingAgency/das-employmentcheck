using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class EmploymentCheckModel
    {
        public EmploymentCheckModel() { }

        public EmploymentCheckModel(
            long employmentCheckId,
            long correlationId,
            string checkType,
            long uln,
            long apprenticeshipId,
            long accountId,
            DateTime minDate,
            DateTime maxDate,
            bool isEmployed,
            DateTime lastUpdated,
            DateTime createdOn)
        {
            EmploymentCheckId = employmentCheckId;
            CorrelationId = correlationId;
            CheckType = checkType;
            Uln = uln;
            ApprenticeshipId = apprenticeshipId;
            AccountId = accountId;
            MinDate = minDate;
            MaxDate = maxDate;
            IsEmployed = isEmployed;
            LastUpdated = lastUpdated;
            CreatedOn = createdOn;
        }

        public long EmploymentCheckId { get; set; }

        public long CorrelationId { get; set; }

        public string CheckType { get; set; }

        public long Uln { get; set; }

        public long ApprenticeshipId { get; set; }

        public long AccountId { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool IsEmployed { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}

