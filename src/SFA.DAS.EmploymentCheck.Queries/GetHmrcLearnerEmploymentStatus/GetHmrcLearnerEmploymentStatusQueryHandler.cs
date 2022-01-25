using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Clients.Hmrc;

namespace SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryHandler
        : IRequestHandler<GetHmrcLearnerEmploymentStatusQueryRequest,
            GetHmrcLearnerEmploymentStatusQueryResult>
    {
        private readonly IHmrcClient _hmrcClient;

        public GetHmrcLearnerEmploymentStatusQueryHandler(
            IHmrcClient hmrcClient,
            ILogger<GetHmrcLearnerEmploymentStatusQueryHandler> logger)
        {
            _hmrcClient = hmrcClient;
        }

        public async Task<GetHmrcLearnerEmploymentStatusQueryResult> Handle(
            GetHmrcLearnerEmploymentStatusQueryRequest request,
            CancellationToken cancellationToken)
        {
            var employmentCheckCacheRequest = await _hmrcClient.CheckEmploymentStatus(request.EmploymentCheckCacheRequest);

            return new GetHmrcLearnerEmploymentStatusQueryResult(employmentCheckCacheRequest);
        }
    }
}
