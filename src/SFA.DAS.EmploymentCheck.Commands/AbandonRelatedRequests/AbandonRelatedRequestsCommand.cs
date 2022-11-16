using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Commands.AbandonRelatedRequests
{
    public class AbandonRelatedRequestsCommand : ICommand
    {
        public AbandonRelatedRequestsCommand(EmploymentCheckCacheRequest[] employmentCheckCacheRequests)
        {
            EmploymentCheckCacheRequests = employmentCheckCacheRequests;
        }

        public EmploymentCheckCacheRequest[] EmploymentCheckCacheRequests { get; }
    }
}
