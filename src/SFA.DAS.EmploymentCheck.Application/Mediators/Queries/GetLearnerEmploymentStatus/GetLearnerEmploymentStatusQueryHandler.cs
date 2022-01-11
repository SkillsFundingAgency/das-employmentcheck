using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using SFA.DAS.EmploymentCheck.Application.Interfaces.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetLearnerEmploymentStatus
{
    public class GetLearnerEmploymentStatusQueryHandler
        : IRequestHandler<GetLearnerEmploymentStatusQueryRequest,
            GetLearnerEmploymentStatusQueryResult>
    {
        private const string ThisClassName = "\n\nGetHmrcEmploymentStatusQueryHandler";

        private readonly IEmploymentCheckClient _employmentCheckClient;
        private readonly ILogger<GetLearnerEmploymentStatusQueryHandler> _logger;

        public GetLearnerEmploymentStatusQueryHandler(
            IEmploymentCheckClient hmrcClient,
            ILogger<GetLearnerEmploymentStatusQueryHandler> logger)
        {
            _employmentCheckClient = hmrcClient;
            _logger = logger;
        }

        public async Task<GetLearnerEmploymentStatusQueryResult> Handle(
            GetLearnerEmploymentStatusQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            EmploymentCheckCacheRequest employmentCheckCacheRequest = null;
            try
            {
                if (request != null &&
                    request.EmploymentCheckCacheRequest != null)
                {
                    employmentCheckCacheRequest = await _employmentCheckClient.CheckEmploymentStatus(request.EmploymentCheckCacheRequest);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: The EmploymentCheckCacheRequest input paramter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetLearnerEmploymentStatusQueryResult(employmentCheckCacheRequest);
        }
    }
}
