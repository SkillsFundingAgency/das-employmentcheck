using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Queries.GetResponseEmploymentCheck
{
    public class GetResponseEmploymentCheckQueryHandler
        : IQueryHandler<GetResponseEmploymentCheckQueryRequest,
            GetResponseEmploymentCheckQueryResult>
    {
        private readonly IEmploymentCheckService _service;

        public GetResponseEmploymentCheckQueryHandler(
            IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<GetResponseEmploymentCheckQueryResult> Handle(
            GetResponseEmploymentCheckQueryRequest request,
            CancellationToken cancellationToken = default
        )
        {
            var employmentCheck = await _service.GetResponseEmploymentCheck();

            return new GetResponseEmploymentCheckQueryResult(employmentCheck);
        }
    }
}