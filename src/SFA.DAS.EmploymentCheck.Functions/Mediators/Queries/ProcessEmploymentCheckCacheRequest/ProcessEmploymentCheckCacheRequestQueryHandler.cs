using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest
{
    public class ProcessEmploymentCheckCacheRequestQueryHandler
        : IRequestHandler<ProcessEmploymentCheckCacheRequestQueryRequest,
            ProcessEmploymentCheckCacheRequestQueryResult>
    {
        #region Private members
        private const string ThisClassName = nameof(ProcessEmploymentCheckCacheRequestQueryHandler);
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        private IEmploymentCheckClient _employmentCheckClient;
        private ILogger<ProcessEmploymentCheckCacheRequestQueryHandler> _logger;
        #endregion Private members

        #region Constructors
        public ProcessEmploymentCheckCacheRequestQueryHandler(
            ILogger<ProcessEmploymentCheckCacheRequestQueryHandler> logger,
            IEmploymentCheckClient employmentCheckMessageQueueClient)
        {
            _logger = logger;
            _employmentCheckClient = employmentCheckMessageQueueClient;
        }
        #endregion Constructors

        #region Handle
        public async Task<ProcessEmploymentCheckCacheRequestQueryResult> Handle(
            ProcessEmploymentCheckCacheRequestQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{nameof(ProcessEmploymentCheckCacheRequestQueryHandler)}.Handle()";

            EmploymentCheckCacheRequest employmentCheckCacheRequest = null;
            try
            {
                employmentCheckCacheRequest = await _employmentCheckClient.ProcessEmploymentCheckCacheRequest();

                if (employmentCheckCacheRequest == null)
                {
                    _logger.LogInformation($"{ThisClassName}: {ErrorMessagePrefix} The value returned from DequeueApprenticeEmploymentCheckMessage_Client() is null.");
                    employmentCheckCacheRequest = new EmploymentCheckCacheRequest(); // create a blank message for the Mediator result wrapper
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{thisMethodName}: {ErrorMessagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new ProcessEmploymentCheckCacheRequestQueryResult(employmentCheckCacheRequest);
        }
        #endregion Handle
    }
}
