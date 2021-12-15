using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto
{
    public class EmploymentCheckCacheResponse
    {
        public long Id { get; set; }

        public long EmploymentCheckId { get; set; }

        public byte RequestTypeId { get; set; }

        public Guid CorrelationId { get; set; }

        public string FoundOnPaye { get; set; }

        public bool ProcessingComplete { get; set; }

        public int Count { get; set; }

        public string HmrcResponse { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
