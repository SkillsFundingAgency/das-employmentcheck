﻿using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommandHandler
        : IRequestHandler<StoreEmploymentCheckResultCommand>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public StoreEmploymentCheckResultCommandHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<StoreEmploymentCheckResultCommandHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<Unit> Handle(
            StoreEmploymentCheckResultCommand request,
            CancellationToken cancellationToken)
        {
            await _employmentCheckClient.StoreEmploymentCheckResult(request.EmploymentCheckCacheRequest);

            return Unit.Value;
        }
    }
}
