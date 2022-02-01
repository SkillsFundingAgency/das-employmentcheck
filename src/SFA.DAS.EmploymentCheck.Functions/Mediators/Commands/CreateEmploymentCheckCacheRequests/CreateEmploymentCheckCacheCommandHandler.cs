using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheCommand>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public CreateEmploymentCheckCacheCommandHandler(IEmploymentCheckClient employmentCheckClient)
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<Unit> Handle(
            CreateEmploymentCheckCacheCommand request,
            CancellationToken cancellationToken)
        {
            await _employmentCheckClient.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);

            return Unit.Value;
        }
    }
}