using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
   public class OldEmploymentCheckCacheRequest
    {
        public long Id { get; set; }

        public Guid CorrelationId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
