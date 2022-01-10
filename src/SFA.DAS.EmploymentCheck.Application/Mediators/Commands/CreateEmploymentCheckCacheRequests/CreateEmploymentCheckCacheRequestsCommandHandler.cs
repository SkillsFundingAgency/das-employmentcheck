using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Interfaces.PaymentsCompliance;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests
{
    public class CreateEmploymentCheckCacheRequestsCommandHandler
        : IRequestHandler<CreateEmploymentCheckCacheRequestCommand>
    {
        #region Private members
        private const string ThisClassName = "\n\nCreateEmploymentCheckCacheRequestsCommandHandler";

        private ILogger<CreateEmploymentCheckCacheRequestsCommandHandler> _logger;
        private IPaymentsComplianceClient _complianceClient;
        #endregion Private members

        #region Constructors
        public CreateEmploymentCheckCacheRequestsCommandHandler(
            IPaymentsComplianceClient complianceClient,
            ILogger<CreateEmploymentCheckCacheRequestsCommandHandler> logger)
        {
            _logger = logger;
            _complianceClient = complianceClient;
        }
        #endregion Constructors

        #region Handle
        public async Task<Unit> Handle(
            CreateEmploymentCheckCacheRequestCommand request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            try
            {
                if (request != null &&
                    request.EmploymentCheckData != null)
                {
                    // Call the application client to create the employment check cache requests
                    await _complianceClient.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData);
                }
                else
                {
                    _logger.LogInformation($"{DateTime.UtcNow} {thisMethodName}: No employment check related data supplied to the employment check cache requests.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return Unit.Value;
        }
        #endregion Handle
    }
}