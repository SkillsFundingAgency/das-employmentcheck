using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecks
{
    public class GetEmploymentChecksQueryHandler
        : IRequestHandler<GetEmploymentChecksQueryRequest,
            GetEmploymentChecksQueryResult>
    {
        private const string ThisClassName = "\n\nGetEmploymentChecksQueryHandler";

        private IEmploymentCheckClient _employmentCheckClient;
        private ILogger<GetEmploymentChecksQueryHandler> _logger;

        public GetEmploymentChecksQueryHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<GetEmploymentChecksQueryHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<GetEmploymentChecksQueryResult> Handle(
            GetEmploymentChecksQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            IList<Application.Models.Domain.EmploymentCheckModel> employmentCheckModels = null;
            try
            {
                // Call the application client to get the employment checks
                employmentCheckModels = await _employmentCheckClient.GetEmploymentChecksBatch(request.EmploymentCheckLastHighestBatchId);

                if (employmentCheckModels != null &&
                    employmentCheckModels.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName} returned {employmentCheckModels.Count} employment check(s)");
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} returned null/zero employment checks");
                    employmentCheckModels = new List<Application.Models.Domain.EmploymentCheckModel>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetEmploymentChecksQueryResult(employmentCheckModels);
        }
    }
}
