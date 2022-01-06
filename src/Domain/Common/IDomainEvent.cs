using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Domain.Common
{
    public interface IHasDomainEvent
    {
        public List<DomainEvent> DomainEvents { get; set; }
    }
}
