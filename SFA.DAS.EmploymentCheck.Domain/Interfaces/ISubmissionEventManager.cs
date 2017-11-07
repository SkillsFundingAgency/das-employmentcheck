using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Domain.Interfaces
{
    public interface ISubmissionEventManager
    {
        long LastProcessedEventId { get; }

        Task PollSubmissionEvents();

        Task DetermineProcessingStartingPoint();
    }
}
