﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    [Table("ApprenticeEmploymentCheck", Schema = "Business")]
    public class EmploymentCheck
    {
        public EmploymentCheck() { }

        public EmploymentCheck(
            long id,
            Guid? correlationId,
            string checkType,
            long uln,
            long? apprenticeshipId,
            long accountId,
            DateTime minDate,
            DateTime maxDate,
            bool? employed,
            DateTime lastUpdatedOn,
            DateTime createdOn)
        {
            Id = id;
            CorrelationId = correlationId;
            CheckType = checkType;
            Uln = uln;
            ApprenticeshipId = apprenticeshipId;
            AccountId = accountId;
            MinDate = minDate;
            MaxDate = maxDate;
            Employed = employed;
            LastUpdatedOn = lastUpdatedOn;
            CreatedOn = createdOn;
        }

        public long Id { get; set; }

        public Guid? CorrelationId { get; set; }

        public string CheckType { get; set; }

        public long Uln { get; set; }

        public long? ApprenticeshipId { get; set; }

        public long AccountId { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public DateTime LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}

