using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SaveEmploymentCheckResult
{
    public class SaveEmploymentCheckResultCommandHandler
        : IRequestHandler<SaveEmploymentCheckResultCommand>
    {
        private const string ThisClassName = "\n\nSaveEmploymentCheckResultCommandHandler";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private IEmploymentCheckClient _employmentCheckClient;
        private readonly ILogger<SaveEmploymentCheckResultCommandHandler> _logger;

        public SaveEmploymentCheckResultCommandHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<SaveEmploymentCheckResultCommandHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            SaveEmploymentCheckResultCommand request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            try
            {
                if (request != null &&
                    request.EmploymentCheckMessage != null)
                {
                    // Call the application client to save the employment check result
                    await _employmentCheckClient.SaveEmploymentCheckResult_Client(request.EmploymentCheckMessage);
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The request input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ThisClassName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return Unit.Value;
        }
    }
}
