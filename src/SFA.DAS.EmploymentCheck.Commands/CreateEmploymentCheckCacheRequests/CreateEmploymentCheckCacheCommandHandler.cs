using MediatR;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheCommand>
    {
        private readonly IEmploymentCheckService _service;

        public CreateEmploymentCheckCacheCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(
            CreateEmploymentCheckCacheCommand request,
            CancellationToken cancellationToken)
        {
            await _service.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);

            return Unit.Value;
        }
    }
}