using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck
{
    public class StoreCompletedEmploymentCheckCommandHandler
        : ICommandHandler<StoreCompletedEmploymentCheckCommand>
    {
        private readonly IEmploymentCheckService _service;

        public StoreCompletedEmploymentCheckCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task Handle(
            StoreCompletedEmploymentCheckCommand command,
            CancellationToken cancellationToken = default
        )
        {
            await _service.StoreCompletedEmploymentCheck(command.EmploymentCheck);
        }
    }
}