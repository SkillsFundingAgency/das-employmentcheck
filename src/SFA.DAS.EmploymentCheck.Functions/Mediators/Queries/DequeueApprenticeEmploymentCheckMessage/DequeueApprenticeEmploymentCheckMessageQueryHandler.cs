using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueApprenticeEmploymentCheckMessage
{
    public class DequeueApprenticeEmploymentCheckMessageQueryHandler
        : IRequestHandler<DequeueApprenticeEmploymentCheckMessageQueryRequest,
            DequeueApprenticeEmploymentCheckMessageQueryResult>
    {
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        private readonly IEmploymentCheckClient _employmentCheckClient;
        private readonly ILogger<DequeueApprenticeEmploymentCheckMessageQueryHandler> _logger;

        public DequeueApprenticeEmploymentCheckMessageQueryHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<DequeueApprenticeEmploymentCheckMessageQueryHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<DequeueApprenticeEmploymentCheckMessageQueryResult> Handle(
            DequeueApprenticeEmploymentCheckMessageQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{nameof(DequeueApprenticeEmploymentCheckMessageQueryHandler)}.Handle()";

            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessage = null;
            try
            {
                // Call the application client to get the apprentices employment check queue messages
                apprenticeEmploymentCheckMessage = await _employmentCheckClient.DequeueApprenticeEmploymentCheckMessage_Client();

                if(apprenticeEmploymentCheckMessage == null)
                {
                    _logger.LogInformation($"{thisMethodName}: The value returned from DequeueApprenticeEmploymentCheckMessage_Client() is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new DequeueApprenticeEmploymentCheckMessageQueryResult(apprenticeEmploymentCheckMessage);
        }
    }
}
