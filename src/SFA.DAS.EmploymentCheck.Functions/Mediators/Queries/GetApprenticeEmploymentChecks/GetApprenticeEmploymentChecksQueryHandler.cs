using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks
{
    public class GetApprenticeEmploymentChecksQueryHandler
        : IRequestHandler<GetApprenticeEmploymentChecksQueryRequest,
            GetApprenticeEmploymentChecksQueryResult>
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

        public async Task<GetApprenticeEmploymentChecksQueryResult> Handle(
            GetApprenticeEmploymentChecksQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentChecks = null;
            try
            {
                // Call the application client to get the apprentices requiring an employment check
                apprenticeEmploymentChecks = await _employmentCheckClient.GetApprenticeEmploymentChecksBatch_Client(request.employmentCheckLastGetId);

                if (apprenticeEmploymentChecks != null &&
                    apprenticeEmploymentChecks.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName} returned {apprenticeEmploymentChecks.Count} apprentice(s)");
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} returned null/zero apprentices");
                    apprenticeEmploymentChecks = new List<ApprenticeEmploymentCheckModel>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetApprenticeEmploymentChecksQueryResult(apprenticeEmploymentChecks);
        }
    }
}
