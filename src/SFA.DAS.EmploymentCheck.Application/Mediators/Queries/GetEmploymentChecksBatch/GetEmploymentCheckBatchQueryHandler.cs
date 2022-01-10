using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Interfaces.PaymentsCompliance;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryHandler
        : IRequestHandler<GetEmploymentCheckBatchQueryRequest,
            GetEmploymentCheckBatchQueryResult>
    {
        private ILogger<GetEmploymentCheckBatchQueryHandler> _logger;
        private IPaymentsComplianceClient _complianceClient;

        public GetEmploymentCheckBatchQueryHandler(
            ILogger<GetEmploymentCheckBatchQueryHandler> logger,
            IPaymentsComplianceClient complianceClient)
        {
            _logger = logger;
            _complianceClient = complianceClient;

        }

        public async Task<GetEmploymentCheckBatchQueryResult> Handle(
            GetEmploymentCheckBatchQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            Guard.Against.Null(request, nameof(request));

            var employmentChecks = await _complianceClient.GetEmploymentChecksBatch();

            if (employmentChecks.Count > 0)
            {
                _logger.LogInformation($"{thisMethodName} returned {employmentChecks.Count} employment check(s)");
            }
            else
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero employment checks");
                employmentChecks = new List<Domain.Entities.EmploymentCheck>(); // return empty list rather than null
            }

            return new GetEmploymentCheckBatchQueryResult(employmentChecks);
        }
    }
}
