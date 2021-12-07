using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks
{
    public class GetApprenticeEmploymentChecksQueryHandler
        : IRequestHandler<GetEmploymentChecksQueryRequest,
            GetEmploymentChecksQueryResult>
    {
        private const string ThisClassName = "\n\nGetApprenticeEmploymentChecksActivity";

        private IEmploymentCheckClient _employmentCheckClient;
        private ILogger<GetApprenticeEmploymentChecksQueryHandler> _logger;

        public GetApprenticeEmploymentChecksQueryHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<GetApprenticeEmploymentChecksQueryHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<GetEmploymentChecksQueryResult> Handle(
            GetEmploymentChecksQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            IList<EmploymentCheckModel> employmentChecks = null;
            try
            {
                // Call the application client to get the apprentices requiring an employment check
                employmentChecks = await _employmentCheckClient.GetEmploymentChecksBatch_Client(request.EmploymentCheckLastHighestBatchId);

                if (employmentChecks != null &&
                    employmentChecks.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName} returned {employmentChecks.Count} apprentice(s)");
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} returned null/zero apprentices");
                    employmentChecks = new List<EmploymentCheckModel>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetEmploymentChecksQueryResult(employmentChecks);
        }
    }
}
