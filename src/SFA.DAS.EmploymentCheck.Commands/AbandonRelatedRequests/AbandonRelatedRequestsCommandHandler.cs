using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Commands.AbandonRelatedRequests
{
    public class AbandonRelatedRequestsCommandHandler : ICommandHandler<AbandonRelatedRequestsCommand>
    {
        private readonly IEmploymentCheckService _service;

        public AbandonRelatedRequestsCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task Handle(AbandonRelatedRequestsCommand command, CancellationToken cancellationToken = default)
        {
            await _service.AbandonRelatedRequests(command.EmploymentCheckCacheRequests);
        }
    }
}