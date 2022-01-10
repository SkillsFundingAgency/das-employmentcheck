using SFA.DAS.EmploymentCheck.Domain.Common;
using System;

namespace SFA.DAS.EmploymentCheck.Domain.Common
{
    public abstract class Entity
        : IEntity
    {
        public long Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
    }
}
