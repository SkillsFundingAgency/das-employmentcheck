using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto
{
    public class DataCollectionsResponse
    {
        public long Id { get; set; }

        public long EmploymentCheckCacheRequestId { get; set; }

        public Guid CorrelationId { get; set; }

        public long Uln { get; set; }

        public string NiNumber { get; set; }

        public string Response { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
