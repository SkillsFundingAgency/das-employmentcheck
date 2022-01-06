using System;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    public class PayeSchemeApiResponse
    {
        public Guid? CorrelationId { get; set; }

        public long AccountId { get; set; }

        public string Response { get; set; }
    }
}
