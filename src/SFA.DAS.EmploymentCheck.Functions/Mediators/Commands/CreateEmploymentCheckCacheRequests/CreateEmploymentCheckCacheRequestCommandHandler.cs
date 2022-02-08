using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheRequestCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheRequestCommand,
            CreateEmploymentCheckCacheRequestCommandResult>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public CreateEmploymentCheckCacheRequestCommandHandler(
            IEmploymentCheckClient employmentCheckClient
        )
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<CreateEmploymentCheckCacheRequestCommandResult> Handle(
            CreateEmploymentCheckCacheRequestCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _employmentCheckClient.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);

            return new CreateEmploymentCheckCacheRequestCommandResult(result.SingleOrDefault());
        }
    }
}