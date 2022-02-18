using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequest
{
    public class CreateEmploymentCheckCacheRequestCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheRequestCommand>
    {
        private readonly IEmploymentCheckService _service;

        public CreateEmploymentCheckCacheRequestCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(
            CreateEmploymentCheckCacheRequestCommand request,
            CancellationToken cancellationToken)
        {
            await _service.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);

            return Unit.Value;
        }
    }
}