using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Domain.Models;

namespace SFA.DAS.EmploymentCheck.Domain.Interfaces
{
    public interface ISubmissionEventRepository
    {
        Task<long> GetLastProcessedEventId();

        Task<IEnumerable<PreviousHandledSubmissionEvent>> GetPreviouslyHandledSubmissionEvents(IEnumerable<long> ulns);

        Task SetLastProcessedEvent(long id);

        Task StoreEmploymentCheckResult(PreviousHandledSubmissionEvent submissionEvent);
    }
}
