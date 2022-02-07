using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus
{
    public class SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommandHandler
        : IRequestHandler<SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommand,
            SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommandResult>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommandHandler(
            IEmploymentCheckClient employmentCheckClient
        )
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommandResult> Handle(
            SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommand command,
            CancellationToken cancellationToken
        )
        {
            var result = await _employmentCheckClient.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus(command.EmploymentCheckCacheRequestAndStatusToSet);

            return new SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatusCommandResult(result);
        }
    }
}