using System;
using System.Collections.Generic;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models
{
    [Table("ClawbackPayment", Schema = "incentives")]
    public partial class ClawbackPayment
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateClawbackCreated { get; set; }
        public DateTime? DateClawbackSent { get; set; }
        public byte? CollectionPeriod { get; set; }
        public short? CollectionPeriodYear { get; set; }
        public SubnominalCode SubnominalCode { get; set; }
        public Guid PaymentId { get; set; }
        public string VrfVendorId { get; set; }
    }
}
