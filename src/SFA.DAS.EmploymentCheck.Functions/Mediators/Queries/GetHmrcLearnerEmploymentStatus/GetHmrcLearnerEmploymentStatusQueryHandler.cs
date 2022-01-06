using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryHandler
        : IRequestHandler<GetHmrcLearnerEmploymentStatusQueryRequest,
            GetHmrcLearnerEmploymentStatusQueryResult>
    {
        private const string ThisClassName = "\n\nGetHmrcEmploymentStatusQueryHandler";

        private IHmrcClient _hmrcClient;
        private ILogger<GetHmrcLearnerEmploymentStatusQueryHandler> _logger;

        public GetHmrcLearnerEmploymentStatusQueryHandler(
            IHmrcClient hmrcClient,
            ILogger<GetHmrcLearnerEmploymentStatusQueryHandler> logger)
        {
            _hmrcClient = hmrcClient;
            _logger = logger;
        }

        public async Task<GetHmrcLearnerEmploymentStatusQueryResult> Handle(
            GetHmrcLearnerEmploymentStatusQueryRequest request,
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

            return new GetHmrcLearnerEmploymentStatusQueryResult(employmentCheckCacheRequest);
        }
    }
}
