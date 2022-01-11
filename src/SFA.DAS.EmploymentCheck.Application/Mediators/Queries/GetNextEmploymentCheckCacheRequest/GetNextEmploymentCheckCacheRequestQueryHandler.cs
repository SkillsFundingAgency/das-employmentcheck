using MediatR;
using SFA.DAS.EmploymentCheck.Application.Interfaces.PaymentsCompliance;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetNextEmploymentCheckCacheRequest
{
    public class GetNextEmploymentCheckCacheRequestQueryHandler
        : IRequestHandler<GetNextEmploymentCheckCacheRequestQueryRequest,
            GetNextEmploymentCheckCacheRequestQueryResult>
    {
        private readonly IPaymentsComplianceClient _paymentsComplianceClient;

        public GetNextEmploymentCheckCacheRequestQueryHandler(
            IPaymentsComplianceClient paymentsComplianceClient)
        {
            _paymentsComplianceClient = paymentsComplianceClient;
        }

        public async Task<GetNextEmploymentCheckCacheRequestQueryResult> Handle(
            GetNextEmploymentCheckCacheRequestQueryRequest request,
            CancellationToken cancellationToken)
        {
            return new GetNextEmploymentCheckCacheRequestQueryResult(await _paymentsComplianceClient.GetNextEmploymentCheckCacheRequest());
        }
    }
}
