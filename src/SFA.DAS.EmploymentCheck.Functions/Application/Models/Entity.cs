using System;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Models
{
    public class Entity
    {
        public Entity(long id, DateTime lastUpdatedOn, DateTime createdOn)
        {
            Id = id;
            LastUpdatedOn = lastUpdatedOn;
            CreatedOn = createdOn;
        }

        public long Id { get; set; }

        public DateTime LastUpdatedOn { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
