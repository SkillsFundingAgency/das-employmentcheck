using MediatR;
using SFA.DAS.EmploymentCheck.Domain.Entities;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommand
        : IRequest
    {
        public StoreEmploymentCheckResultCommand(EmploymentCheckCacheRequest employmentCheckCacheRequest)
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
