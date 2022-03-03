using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest
{
    public class CreateEmploymentCheckCacheRequestCommandHandler
        : ICommandHandler<CreateEmploymentCheckCacheRequestCommand>
    {
        private readonly IEmploymentCheckService _service;

        public CreateEmploymentCheckCacheRequestCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task Handle(
            CreateEmploymentCheckCacheRequestCommand request,
            CancellationToken cancellationToken)
        {
            await _service.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);
        }
    }
}