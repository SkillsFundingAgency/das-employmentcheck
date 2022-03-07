using MediatR;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck
{
    public class StoreCompletedEmploymentCheckCommandHandler
        : IRequestHandler<StoreCompletedEmploymentCheckCommand>
    {
        private readonly IEmploymentCheckService _service;

        public StoreCompletedEmploymentCheckCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(
            StoreCompletedEmploymentCheckCommand command,
            CancellationToken cancellationToken
        )
        {
            await _service.StoreCompletedCheck(command.EmploymentCheck);

            return Unit.Value;
        }
    }
}