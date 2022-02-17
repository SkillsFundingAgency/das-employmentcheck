using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryHandler
        : IRequestHandler<GetEmploymentCheckBatchQueryRequest,
            GetEmploymentCheckBatchQueryResult>
    {
        private readonly IEmploymentCheckService _service;

        public GetEmploymentCheckBatchQueryHandler(
            IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<GetEmploymentCheckBatchQueryResult> Handle(
            GetEmploymentCheckBatchQueryRequest request,
            CancellationToken cancellationToken)
        {
            var employmentChecks = await _service.GetEmploymentChecksBatch();

            return new GetEmploymentCheckBatchQueryResult(employmentChecks);
        }
    }
}
