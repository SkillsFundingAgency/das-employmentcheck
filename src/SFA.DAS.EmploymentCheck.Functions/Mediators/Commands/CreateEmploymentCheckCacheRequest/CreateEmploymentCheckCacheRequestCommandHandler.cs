using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequest
{
    public class CreateEmploymentCheckCacheRequestCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheRequestCommand>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public CreateEmploymentCheckCacheRequestCommandHandler(IEmploymentCheckClient employmentCheckClient)
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<Unit> Handle(
            CreateEmploymentCheckCacheRequestCommand request,
            CancellationToken cancellationToken)
        {
            await _employmentCheckClient.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);

            return Unit.Value;
        }
    }
}