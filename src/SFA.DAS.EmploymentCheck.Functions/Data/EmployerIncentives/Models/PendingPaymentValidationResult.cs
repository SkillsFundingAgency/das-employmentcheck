using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmploymentCheck.Functions.Data.EmployerIncentives.Models
{
    public partial class PendingPaymentValidationResult
    {
        public Guid Id { get; set; }
        public Guid PendingPaymentId { get; set; }
        public string Step { get; set; }
        public bool Result { get; set; }
        public byte PeriodNumber { get; set; }
        public short PaymentYear { get; set; }
        public DateTime CreatedDateUtc { get; set; }
    }
}
