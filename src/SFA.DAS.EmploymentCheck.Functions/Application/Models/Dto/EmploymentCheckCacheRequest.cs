using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto
{
    [Table("Cache.EmploymentCheckCacheRequest")]

    public class EmploymentCheckCacheRequest
    {
        public long Id { get; set; }

        public long EmploymentCheckId { get; set; }

        public byte RequestTypeId { get; set; }

        public Guid CorrelationId { get; set; }

        public string Description { get; set; }

        public string Data { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
