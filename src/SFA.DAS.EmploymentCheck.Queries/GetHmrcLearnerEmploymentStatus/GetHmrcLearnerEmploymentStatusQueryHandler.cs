using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryHandler
        : IRequestHandler<GetHmrcLearnerEmploymentStatusQueryRequest,
            GetHmrcLearnerEmploymentStatusQueryResult>
    {
        private readonly IHmrcService _service;

        public GetHmrcLearnerEmploymentStatusQueryHandler(IHmrcService service)
        {
            _service = service;
        }

        public async Task<GetHmrcLearnerEmploymentStatusQueryResult> Handle(
            GetHmrcLearnerEmploymentStatusQueryRequest request,
            CancellationToken cancellationToken)
        {
            var employmentCheckCacheRequest = await _service.IsNationalInsuranceNumberRelatedToPayeScheme(request.EmploymentCheckCacheRequest);

            return new GetHmrcLearnerEmploymentStatusQueryResult(employmentCheckCacheRequest);
        }
    }
}
