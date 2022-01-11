using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheCommand>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public CreateEmploymentCheckCacheCommandHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<CreateEmploymentCheckCacheCommandHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
        }

        #region Handle

        public async Task<Unit> Handle(
            CreateEmploymentCheckCacheCommand request,
            CancellationToken cancellationToken)
        {

            await _employmentCheckClient.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);

            return Unit.Value;
        }

        #endregion Handle
    }
}