using System;
using System.Collections.Generic;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models
{
    [Table("PendingPayment", Schema = "incentives")]
    public partial class PendingPayment
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CalculatedDate { get; set; }
        public DateTime? PaymentMadeDate { get; set; }
        public byte? PeriodNumber { get; set; }
        public short? PaymentYear { get; set; }
        public long AccountLegalEntityId { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public EarningType EarningType { get; set; }
        public bool ClawedBack { get; set; }

        public ICollection<PendingPaymentValidationResult> ValidationResults { get; set; }

        public PendingPayment()
        {
            ValidationResults = new List<PendingPaymentValidationResult>();
        }
    }
}
