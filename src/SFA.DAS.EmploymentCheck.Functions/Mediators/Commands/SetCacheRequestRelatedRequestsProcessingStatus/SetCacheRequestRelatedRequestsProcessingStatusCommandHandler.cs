using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus
{
    public class SetCacheRequestRelatedRequestsProcessingStatusCommandHandler
        : IRequestHandler<SetCacheRequestRelatedRequestsProcessingStatusCommand,
            SetCacheRequestRelatedRequestsProcessingStatusCommandResult>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public SetCacheRequestRelatedRequestsProcessingStatusCommandHandler(
            IEmploymentCheckClient employmentCheckClient
        )
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<SetCacheRequestRelatedRequestsProcessingStatusCommandResult> Handle(
            SetCacheRequestRelatedRequestsProcessingStatusCommand command,
            CancellationToken cancellationToken
        )
        {
            var result = await _employmentCheckClient.SetCacheRequestRelatedRequestsProcessingStatus(command.EmploymentCheckCacheRequestAndStatusToSet);

            return new SetCacheRequestRelatedRequestsProcessingStatusCommandResult(result);
        }
    }
}