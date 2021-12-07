using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
    public class AccountsResponse
    {
        public long Id { get; set; }

        public Guid CorrelationId { get; set; }

        public long AccountId { get; set; }

        public string PayeSchemes { get; set; }

        public string Response { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
