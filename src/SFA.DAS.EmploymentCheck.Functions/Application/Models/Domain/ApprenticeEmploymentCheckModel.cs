using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class ApprenticeEmploymentCheckModel
    {
        public ApprenticeEmploymentCheckModel(
            long id,
            long uln,
            string nationalInsuranceNumber,
            long ukprn,
            long apprenticeshipId,
            long accountId,
            DateTime minDate,
            DateTime maxDate,
            string checkType,
            bool isEmployed,
            DateTime lastUpdated,
            DateTime createdDate,
            bool hasBeenChecked)
        {
            Id = id;
            ULN = uln;
            NationalInsuranceNumber = nationalInsuranceNumber;
            UKPRN = ukprn;
            ApprenticeshipId = apprenticeshipId;
            AccountId = accountId;
            MinDate = minDate;
            MaxDate = maxDate;
            CheckType = checkType;
            IsEmployed = isEmployed;
            LastUpdated = lastUpdated;
            CreatedDate = createdDate;
            HasBeenChecked = hasBeenChecked;
        }

        public long Id { get; set; }

        public long ULN { get; set; }

        public long UKPRN { get; set; }

        public long ApprenticeshipId { get; set; }

        public long AccountId { get; set; }

        public string NationalInsuranceNumber { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public string CheckType { get; set; }

        public bool IsEmployed { get; set; }

        public DateTime LastUpdated { get; set; }

        public DateTime CreatedDate { get; set; }
      
        public bool HasBeenChecked { get; set; }
    }
}

