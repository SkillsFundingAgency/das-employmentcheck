using System;

namespace SFA.DAS.EmploymentCheck.Domain.Common
{
    public interface IAuditableEntity
    {
        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
    }
}
