using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;
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
        private readonly IEmploymentCheckCacheRequestFactory _cacheRequestFactory;

        public CreateEmploymentCheckCacheRequestCommandHandler(
            IEmploymentCheckClient employmentCheckClient,
            IEmploymentCheckCacheRequestFactory cacheRequestFactory
        )
        {
            _employmentCheckClient = employmentCheckClient;
            _cacheRequestFactory = cacheRequestFactory;
        }

        public async Task<CreateEmploymentCheckCacheRequestCommandResult> Handle(
            CreateEmploymentCheckCacheRequestCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _employmentCheckClient.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData, _cacheRequestFactory);
            if(result == null)
            {
                return new CreateEmploymentCheckCacheRequestCommandResult(new EmploymentCheckCacheRequest());
            }

            return new CreateEmploymentCheckCacheRequestCommandResult(result.SingleOrDefault());
        }
    }
}