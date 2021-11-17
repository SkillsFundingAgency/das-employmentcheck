﻿using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class ApprenticeEmploymentCheckModel
    {
        public ApprenticeEmploymentCheckModel() { }

        public ApprenticeEmploymentCheckModel(
            long id,
            long uln,
            long ukprn,
            long apprenticeshipId,
            long accountId,
            string nationalInsuranceNumber,
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
            UKPRN = ukprn;
            ApprenticeshipId = apprenticeshipId;
            AccountId = accountId;
            NationalInsuranceNumber = nationalInsuranceNumber;
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

