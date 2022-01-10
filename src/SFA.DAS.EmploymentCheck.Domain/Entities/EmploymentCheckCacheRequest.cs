using System;
using Dapper.Contrib.Extensions;
using SFA.DAS.EmploymentCheck.Domain.Common;

namespace SFA.DAS.EmploymentCheck.Domain.Entities
{
    [Table("Cache.EmploymentCheckCacheRequest")]
    public class EmploymentCheckCacheRequest
        : Entity
    {
        public long Id { get; set; }

        public long EmploymentCheckId { get; set; }

        public Guid? CorrelationId { get; set; }

        public string Nino { get; set; }

        public string PayeScheme { get; set; }

        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public bool? Employed { get; set; }

        public short? RequestCompletionStatus { get; set; }
    }
}
