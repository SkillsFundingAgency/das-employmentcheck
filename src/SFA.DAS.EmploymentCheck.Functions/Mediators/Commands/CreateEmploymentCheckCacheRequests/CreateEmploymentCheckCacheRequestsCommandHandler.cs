using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheRequestsCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheRequestsCommandRequest,
            CreateEmploymentCheckCacheRequestsCommandResult>
    {
        private const string ThisClassName = "\n\nCreateEmploymentCheckCacheRequestsQueryHandler";

        private IEmploymentCheckClient _employmentCheckClient;
        private ILogger<CreateEmploymentCheckCacheRequestsCommandHandler> _logger;

        public CreateEmploymentCheckCacheRequestsCommandHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<CreateEmploymentCheckCacheRequestsCommandHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<CreateEmploymentCheckCacheRequestsCommandResult> Handle(
            CreateEmploymentCheckCacheRequestsCommandRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            IList<EmploymentCheckCacheRequest> employmentCheckCacheRequests = null;
            try
            {
                // Call the application client to create the EmploymentCheckCacheRequests
                employmentCheckCacheRequests = await _employmentCheckClient.CreateEmploymentCheckCacheRequests(request.EmploymentCheckModels);

                if (employmentCheckCacheRequests != null &&
                    employmentCheckCacheRequests.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName} returned {employmentCheckCacheRequests.Count} employment check(s)");
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} returned null/zero employment checks");
                    employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new CreateEmploymentCheckCacheRequestsCommandResult(employmentCheckCacheRequests);
        }
    }
}
