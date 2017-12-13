using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Domain.Models;

namespace SFA.DAS.EmploymentCheck.Domain.Interfaces
{
    public interface ISubmissionEventRepository
    {
        Task<long> GetPollingProcessingStartingPoint();

        Task<IEnumerable<PreviousHandledSubmissionEvent>> GetPreviouslyHandledSubmissionEvents(IEnumerable<long> ulns);
    }
}
