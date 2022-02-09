using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommand
        : IRequest<StoreEmploymentCheckResultCommandResult>
    {
        public StoreEmploymentCheckResultCommand(
            EmploymentCheckCacheRequest employmentCheckCacheRequest
        )
        {
            EmploymentCheckCacheRequest = employmentCheckCacheRequest;
        }

        public EmploymentCheckCacheRequest EmploymentCheckCacheRequest { get; }
    }
}
