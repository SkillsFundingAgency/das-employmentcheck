﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest
{
    public class CreateEmploymentCheckCacheRequestCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheRequestCommand>
    {
        private readonly IEmploymentCheckService _service;

        public CreateEmploymentCheckCacheRequestCommandHandler(IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<Unit> Handle(
            CreateEmploymentCheckCacheRequestCommand request,
            CancellationToken cancellationToken)
        {
            await _service.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);

            return Unit.Value;
        }
    }
}