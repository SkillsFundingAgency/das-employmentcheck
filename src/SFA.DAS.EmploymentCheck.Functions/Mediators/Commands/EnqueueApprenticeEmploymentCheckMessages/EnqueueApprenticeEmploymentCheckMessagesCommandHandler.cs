using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice
{
    public class EnqueueApprenticeEmploymentCheckMessagesCommandHandler
        : IRequestHandler<EnqueueApprenticeEmploymentCheckMessagesCommand>
    {
        private const string ThisClassName = "\n\nEnqueueApprenticeEmploymentCheckMessagesCommandHandler";

        private IEmploymentCheckClient _employmentCheckClient;
        private ILogger<EnqueueApprenticeEmploymentCheckMessagesCommandHandler> _logger;

        public EnqueueApprenticeEmploymentCheckMessagesCommandHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<EnqueueApprenticeEmploymentCheckMessagesCommandHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            EnqueueApprenticeEmploymentCheckMessagesCommand request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            try
            {
                if (request != null &&
                    request.ApprenticeRelatedData != null)
                {
                    // Call the application client to store the apprentices employment check queue messages
                    await _employmentCheckClient.EnqueueApprenticeEmploymentCheckMessages_Client(request.ApprenticeRelatedData);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No apprentice related data supplied to create queue messaages.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return Unit.Value;
        }
    }
}
