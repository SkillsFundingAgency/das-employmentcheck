using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommandHandler
        : IRequestHandler<StoreEmploymentCheckResultCommand,
            StoreEmploymentCheckResultCommandResult>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public StoreEmploymentCheckResultCommandHandler(IEmploymentCheckClient employmentCheckClient)
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<StoreEmploymentCheckResultCommandResult> Handle(
            StoreEmploymentCheckResultCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _employmentCheckClient.StoreEmploymentCheckResult(request.EmploymentCheckCacheRequest);

            return new StoreEmploymentCheckResultCommandResult(result);
        }
    }
}
