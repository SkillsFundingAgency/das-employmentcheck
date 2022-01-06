using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryHandler
        : IRequestHandler<GetEmploymentCheckBatchQueryRequest,
            GetEmploymentCheckBatchQueryResult>
    {
        private ILogger<GetEmploymentCheckBatchQueryHandler> _logger;
        private IEmploymentCheckClient _employmentCheckClient;

        public GetEmploymentCheckBatchQueryHandler(
            ILogger<GetEmploymentCheckBatchQueryHandler> logger,
            IEmploymentCheckClient employmentCheckClient)
        {
            _logger = logger;
            _employmentCheckClient = employmentCheckClient;

        }

        public async Task<GetEmploymentCheckBatchQueryResult> Handle(
            GetEmploymentCheckBatchQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            Guard.Against.Null(request, nameof(request));

            IList<Application.Models.EmploymentCheck> employmentChecks = null;
            try
            {
                // Call the application client to get the employment checks
                employmentChecks = await _employmentCheckClient.GetEmploymentChecksBatch();

                if (employmentChecks != null &&
                    employmentChecks.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName} returned {employmentChecks.Count} employment check(s)");
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} returned null/zero employment checks");
                    employmentChecks = new List<Application.Models.EmploymentCheck>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetEmploymentCheckBatchQueryResult(employmentChecks);
        }
    }
}
