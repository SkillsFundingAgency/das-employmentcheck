using System;
using System.Collections.Generic;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models
{
    [Table("ApprenticeshipIncentive", Schema = "incentives")]
    public partial class ApprenticeshipIncentive
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public long ULN { get; set; }
        public long? UKPRN { get; set; }
        public ApprenticeshipEmployerType EmployerType { get; set; }
        public DateTime StartDate { get; set; }
        public Guid IncentiveApplicationApprenticeshipId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public bool RefreshedLearnerForEarnings { get; set; }
        public bool HasPossibleChangeOfCircumstances { get; set; }
        public bool PausePayments { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public string SubmittedByEmail { get; set; }
        public string CourseName { get; set; }

        public ICollection<PendingPayment> PendingPayments { get; set; }
        public ICollection<Payment> Payments { get; set; }

        public ICollection<ClawbackPayment> ClawbackPayments { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public IncentiveStatus Status { get; set; }


        public ICollection<ApprenticeshipBreakInLearning> BreakInLearnings { get; set; }

        public int? MinimumAgreementVersion { get; set; }
        public DateTime? EmploymentStartDate { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public Phase Phase { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public WithdrawnBy? WithdrawnBy { get; set; }

        public ApprenticeshipIncentive()
        {
            PendingPayments = new List<PendingPayment>();
            Payments = new List<Payment>();
            ClawbackPayments = new List<ClawbackPayment>();
            BreakInLearnings = new List<ApprenticeshipBreakInLearning>();
        }
    }
}
