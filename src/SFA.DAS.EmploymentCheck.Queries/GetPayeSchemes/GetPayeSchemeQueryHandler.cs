using Ardalis.GuardClauses;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes
{
    public class GetPayeSchemeQueryHandler : IQueryHandler<GetPayeSchemesQueryRequest, GetPayeSchemesQueryResult>
    {
        private readonly IEmployerAccountService _service;

        public GetPayeSchemeQueryHandler(IEmployerAccountService service)
        {
            _service = service;
        }

        public async Task<GetPayeSchemesQueryResult> Handle(
            GetPayeSchemesQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.Null(request.EmploymentCheck, nameof(request.EmploymentCheck));

            var result = await _service.GetEmployerPayeSchemes(request.EmploymentCheck);

            return new GetPayeSchemesQueryResult(result);
        }
    }
}