using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryHandler
        : IRequestHandler<GetHmrcLearnerEmploymentStatusQueryRequest,
            GetHmrcLearnerEmploymentStatusQueryResult>
    {
        private readonly IHmrcService _hmrcService;

        public GetHmrcLearnerEmploymentStatusQueryHandler(
            IHmrcService hmrcService,
            ILogger<GetHmrcLearnerEmploymentStatusQueryHandler> logger)
        {
            _hmrcService = hmrcService;
        }

        public async Task<GetHmrcLearnerEmploymentStatusQueryResult> Handle(
            GetHmrcLearnerEmploymentStatusQueryRequest request,
            CancellationToken cancellationToken)
        {
            var employmentCheckCacheRequest = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(request.EmploymentCheckCacheRequest);

            return new GetHmrcLearnerEmploymentStatusQueryResult(employmentCheckCacheRequest);
        }
    }
}
