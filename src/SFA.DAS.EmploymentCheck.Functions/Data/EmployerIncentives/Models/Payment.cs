using System;
using System.Collections.Generic;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models
{
    [Table("Payment", Schema = "incentives")]
    public partial class Payment
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public Guid PendingPaymentId { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public decimal Amount { get; set; }
        public DateTime CalculatedDate { get; set; }
        public DateTime? PaidDate { get; set; }
        public SubnominalCode SubnominalCode { get; set; }
        public byte PaymentPeriod { get; set; }
        public short PaymentYear { get; set; }
        public string VrfVendorId { get; set; }
    }
}
