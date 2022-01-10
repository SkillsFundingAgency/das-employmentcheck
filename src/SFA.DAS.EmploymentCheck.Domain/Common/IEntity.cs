using System;

namespace SFA.DAS.EmploymentCheck.Domain.Common
{
    public interface IEntity
    {
        public long Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }
    }
}