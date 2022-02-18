using SFA.DAS.EmploymentCheck.Application.Clients.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries.GetEmploymentChecksBatch
{
    public class GetEmploymentCheckBatchQueryHandler : IQueryHandler<GetEmploymentCheckBatchQueryRequest, GetEmploymentCheckBatchQueryResult>
    {
        private readonly IEmploymentCheckClient _employmentCheckClient;

        public GetEmploymentCheckBatchQueryHandler(IEmploymentCheckClient employmentCheckClient)
        {
            _employmentCheckClient = employmentCheckClient;
        }

        public async Task<GetEmploymentCheckBatchQueryResult> Handle(GetEmploymentCheckBatchQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var employmentChecks = await _employmentCheckClient.GetEmploymentChecksBatch();

            return new GetEmploymentCheckBatchQueryResult(employmentChecks);
        }
    }
}
