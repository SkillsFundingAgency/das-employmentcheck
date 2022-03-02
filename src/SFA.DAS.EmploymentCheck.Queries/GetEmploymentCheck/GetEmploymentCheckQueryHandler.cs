using MediatR;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck
{
    public class GetEmploymentCheckQueryHandler
        : IRequestHandler<GetEmploymentCheckQueryRequest,
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