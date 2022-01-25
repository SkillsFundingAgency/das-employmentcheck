using MediatR;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommand : IRequest
    {
        public StoreEmploymentCheckResultCommand(
            EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
