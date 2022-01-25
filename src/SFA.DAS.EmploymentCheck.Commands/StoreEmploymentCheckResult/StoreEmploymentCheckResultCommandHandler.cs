using MediatR;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommandHandler
        : IRequestHandler<StoreEmploymentCheckResultCommand>
    {
        private readonly IEmploymentCheckService _service;

        public StoreEmploymentCheckResultCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(
            StoreEmploymentCheckResultCommand request,
            CancellationToken cancellationToken)
        {
            await _service.StoreEmploymentCheckResult(request.EmploymentCheckCacheRequest);

            return Unit.Value;
        }
    }
}
