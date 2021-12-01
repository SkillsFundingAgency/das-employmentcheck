using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueEmploymentCheckMessage
{
    public class DequeueEmploymentCheckMessageQueryHandler
        : IRequestHandler<DequeueEmploymentCheckMessageQueryRequest,
            DequeueEmploymentCheckMessageQueryResult>
    {
        private const string ThisClassName = "\n\nDequeueApprenticeEmploymentCheckMessagesQueryHandler";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";

        private IEmploymentCheckClient _employmentCheckClient;
        private ILogger<DequeueEmploymentCheckMessageQueryHandler> _logger;

        public DequeueEmploymentCheckMessageQueryHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<DequeueEmploymentCheckMessageQueryHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<DequeueEmploymentCheckMessageQueryResult> Handle(
            DequeueEmploymentCheckMessageQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            EmploymentCheckMessage apprenticeEmploymentCheckMessage = null;
            try
            {
                // Call the application client to get the apprentices employment check queue messages
                apprenticeEmploymentCheckMessage = await _employmentCheckClient.DequeueEmploymentCheckMessage_Client();

                if(apprenticeEmploymentCheckMessage == null)
                {
                    _logger.LogInformation($"{ThisClassName}: {ErrorMessagePrefix} The value returned from DequeueApprenticeEmploymentCheckMessage_Client() is null.");
                    apprenticeEmploymentCheckMessage = new EmploymentCheckMessage(); // create a blank message for the Mediator result wrapper
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ThisClassName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new DequeueEmploymentCheckMessageQueryResult(apprenticeEmploymentCheckMessage);
        }
    }
}
