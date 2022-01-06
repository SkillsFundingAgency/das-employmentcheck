using System;

namespace SFA.DAS.EmploymentCheck.Domain.Common
{
    public class AuditableEntity
        : IIdentityEntity,
        IAuditableEntity
    {
        public long Id { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        public DateTime? LastUpdatedOn { get; set; } = DateTime.Now;
    }
}
