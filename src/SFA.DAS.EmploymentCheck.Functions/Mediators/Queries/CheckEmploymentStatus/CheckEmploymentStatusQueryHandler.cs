using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckEmploymentStatus
{
    public class CheckEmploymentStatusQueryHandler
        : IRequestHandler<CheckEmploymentStatusQueryRequest,
            CheckEmploymentStatusQueryResult>
    {
        private const string ThisClassName = "\n\nCheckEmploymentStatusQueryHandler";

        private IHmrcClient _hmrcClient;
        private ILogger<CheckEmploymentStatusQueryHandler> _logger;

        public CheckEmploymentStatusQueryHandler(
            IHmrcClient hmrcClient,
            ILogger<CheckEmploymentStatusQueryHandler> logger)
        {
            _hmrcClient = hmrcClient;
            _logger = logger;
        }

        public async Task<CheckEmploymentStatusQueryResult> Handle(
            CheckEmploymentStatusQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            EmploymentCheckMessage employmentCheckMessageResult = null;
            try
            {
                if (request != null &&
                    request.EmploymentCheckMessage != null)
                {
                    // Call the application client to store the employment check queue messages
                    employmentCheckMessageResult = await _hmrcClient.CheckEmploymentStatus_Client(request.EmploymentCheckMessage);
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

            return new CheckEmploymentStatusQueryResult(employmentCheckMessageResult);
        }
    }
}
