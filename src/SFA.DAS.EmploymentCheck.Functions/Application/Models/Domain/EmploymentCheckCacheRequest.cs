using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain
{
   public class EmploymentCheckCacheRequest
    {
        public long Id { get; set; }

        public int CorrelationId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
