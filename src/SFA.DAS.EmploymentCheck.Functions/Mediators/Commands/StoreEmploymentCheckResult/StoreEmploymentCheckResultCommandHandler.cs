using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult
{
    public class StoreEmploymentCheckResultCommandHandler
        : IRequestHandler<StoreEmploymentCheckResultCommand>
    {
        private const string ThisClassName = "\n\nSaveEmploymentCheckResultCommandHandler";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private IEmploymentCheckClient _employmentCheckClient;
        private readonly ILogger<StoreEmploymentCheckResultCommandHandler> _logger;

        public StoreEmploymentCheckResultCommandHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<StoreEmploymentCheckResultCommandHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<Unit> Handle(
            StoreEmploymentCheckResultCommand request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            try
            {
                if (request != null &&
                    request.EmploymentCheckCacheRequest != null)
                {
                    // Call the application client to save the employment check result
                    await _employmentCheckClient.StoreEmploymentCheckResult(request.EmploymentCheckCacheRequest);
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
