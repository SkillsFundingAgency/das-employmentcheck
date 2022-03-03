using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;

namespace SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryHandler
        : IQueryHandler<GetHmrcLearnerEmploymentStatusQueryRequest,
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
