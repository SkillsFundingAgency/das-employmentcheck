using SFA.DAS.EmploymentCheck.Domain.Common;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Common.Interfaces
{
    public interface IDomainEventService
    {
        Task Publish(DomainEvent domainEvent);
    }
}
