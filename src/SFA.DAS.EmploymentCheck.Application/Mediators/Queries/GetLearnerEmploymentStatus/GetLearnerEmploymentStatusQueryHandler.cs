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

        private IEmploymentCheckClient _hmrcClient;
        private ILogger<GetLearnerEmploymentStatusQueryHandler> _logger;

        public GetLearnerEmploymentStatusQueryHandler(
            IEmploymentCheckClient hmrcClient,
            ILogger<GetLearnerEmploymentStatusQueryHandler> logger)
        {
            _hmrcClient = hmrcClient;
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
                    // Call the application client to store the employment check queue messages
                    employmentCheckCacheRequest = await _hmrcClient.CheckEmploymentStatus(request.EmploymentCheckCacheRequest);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No employment check data was supplied to create the queue messages.");
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
