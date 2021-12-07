using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckEmploymentStatus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus
{
    public class CheckApprenticeEmploymentStatusQueryHandler
        : IRequestHandler<CheckEmploymentStatusQueryRequest,
            CheckEmploymentStatusQueryResult>
    {
        private const string ThisClassName = "\n\nCheckApprenticeEmploymentStatusCommandHandler";

        private IHmrcClient _hmrcClient;
        private ILogger<CheckApprenticeEmploymentStatusQueryHandler> _logger;

        public CheckApprenticeEmploymentStatusQueryHandler(
            IHmrcClient hmrcClient,
            ILogger<CheckApprenticeEmploymentStatusQueryHandler> logger)
        {
            _hmrcClient = hmrcClient;
            _logger = logger;
        }

        public async Task<CheckEmploymentStatusQueryResult> Handle(
            CheckEmploymentStatusQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            EmploymentCheckMessage result = null;
            try
            {
                if (request != null &&
                    request.EmploymentCheckMessage != null)
                {
                    // Call the application client to store the apprentices employment check queue messages
                    result = await _hmrcClient.CheckEmploymentStatus_Client(request.EmploymentCheckMessage);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No apprentice related data supplied to create queue messages.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new CheckEmploymentStatusQueryResult(result);
        }
    }
}
