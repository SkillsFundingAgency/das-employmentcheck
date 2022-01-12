using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace SFA.DAS.EmploymentCheck.Api.Commands
{
    public class RegisterCheckCommandHandler : IRequestHandler<RegisterCheckCommand, RegisterCheckResult>
    {
        public async Task<RegisterCheckResult> Handle(RegisterCheckCommand command,
            CancellationToken cancellationToken)
        {
            return null;
        }
    }
}