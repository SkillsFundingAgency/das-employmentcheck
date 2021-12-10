using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SaveApprenticeEmploymentCheckResult
{
    public class SaveApprenticeEmploymentCheckResultCommandHandler
        : IRequestHandler<SaveApprenticeEmploymentCheckResultCommand>
    {
        private const string ThisClassName = "\n\nSaveApprenticeEmploymentCheckResultCommandHandler";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private IEmploymentCheckClient _employmentCheckClient;
        private readonly ILogger<SaveApprenticeEmploymentCheckResultCommandHandler> _logger;

        public SaveApprenticeEmploymentCheckResultCommandHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<SaveApprenticeEmploymentCheckResultCommandHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            SaveApprenticeEmploymentCheckResultCommand request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            try
            {
                if (request != null &&
                    request.ApprenticeEmploymentCheckMessageModel != null)
                {
                    // Call the application client to save the employment check result
                    await _employmentCheckClient.SaveApprenticeEmploymentCheckResult_Client(request.ApprenticeEmploymentCheckMessageModel);
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: {ErrorMessagePrefix} The ApprenticeEmploymentCheckMessageModel input parameter is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{ThisClassName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return Unit.Value;
        }
    }
}
