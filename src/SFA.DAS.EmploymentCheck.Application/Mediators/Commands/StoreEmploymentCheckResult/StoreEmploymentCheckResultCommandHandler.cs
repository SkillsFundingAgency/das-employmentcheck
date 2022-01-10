using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Interfaces.PaymentsCompliance;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommandHandler
        : IRequestHandler<StoreEmploymentCheckResultCommand>
    {
        private IPaymentsComplianceClient _paymentsComplianceClient;

        public StoreEmploymentCheckResultCommandHandler(IPaymentsComplianceClient paymentsComplianceClient)
        {
            _paymentsComplianceClient = paymentsComplianceClient;
        }

        public async Task<Unit> Handle(
            StoreEmploymentCheckResultCommand request,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(request, nameof(request));

            await _paymentsComplianceClient.StoreEmploymentCheckResult(request.EmploymentCheckCacheRequest);

            return await Task.FromResult(Unit.Value);
        }
    }
}
