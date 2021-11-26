using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueApprenticeEmploymentCheckMessage
{
    public class DequeueApprenticeEmploymentCheckMessageQueryHandler
        : IRequestHandler<DequeueApprenticeEmploymentCheckMessageQueryRequest,
            DequeueApprenticeEmploymentCheckMessageQueryResult>
    {
        private const string ThisClassName = "\n\nDequeueApprenticeEmploymentCheckMessagesQueryHandler";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private IEmploymentCheckClient _employmentCheckClient;
        private ILogger<DequeueApprenticeEmploymentCheckMessageQueryHandler> _logger;

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
            var thisMethodName = $"{ThisClassName}.Handle()";

            EmploymentCheckMessageModel apprenticeEmploymentCheckMessage = null;
            try
            {
                // Call the application client to get the apprentices employment check queue messages
                apprenticeEmploymentCheckMessage = await _employmentCheckClient.DequeueApprenticeEmploymentCheckMessage_Client();

                if(apprenticeEmploymentCheckMessage == null)
                {
                    _logger.LogInformation($"{ThisClassName}: {ErrorMessagePrefix} The value returned from DequeueApprenticeEmploymentCheckMessage_Client() is null.");
                    apprenticeEmploymentCheckMessage = new EmploymentCheckMessageModel(); // create a blank message for the Mediator result wrapper
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{ThisClassName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new DequeueApprenticeEmploymentCheckMessageQueryResult(apprenticeEmploymentCheckMessage);
        }
    }
}
