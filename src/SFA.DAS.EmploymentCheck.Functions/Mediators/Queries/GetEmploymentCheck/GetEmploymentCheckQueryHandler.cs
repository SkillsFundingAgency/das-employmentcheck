using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
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
            ILogger<GetEmploymentCheckQueryHandler> logger,
            IEmploymentCheckClient employmentCheckClient)
        {
            _service = service;
        }

        public async Task<GetEmploymentCheckQueryResult> Handle(
            GetEmploymentCheckQueryRequest request,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(request, nameof(request));

            var employmentChecks = await _employmentCheckClient.GetEmploymentCheck();

            return new GetEmploymentCheckQueryResult(employmentChecks);
        }
    }
}
