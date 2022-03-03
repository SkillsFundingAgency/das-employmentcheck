using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck
{
    public class GetEmploymentCheckQueryHandler
        : IQueryHandler<GetEmploymentCheckQueryRequest,
            GetEmploymentCheckQueryResult>
    {
        private readonly IEmploymentCheckService _service;

        public GetEmploymentCheckQueryHandler(
            IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<GetEmploymentCheckQueryResult> Handle(
            GetEmploymentCheckQueryRequest request,
            CancellationToken cancellationToken
        )
        {
            var employmentCheck = await _service.GetEmploymentCheck();

            return new GetEmploymentCheckQueryResult(employmentCheck);
        }
    }
}