﻿using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus
{
    public class CheckApprenticeEmploymentStatusQueryHandler
        : IRequestHandler<CheckApprenticeEmploymentStatusQueryRequest,
            CheckApprenticeEmploymentStatusQueryResult>
    {
        private const string ThisClassName = "\n\nCheckApprenticeEmploymentStatusCommandHandler";

        private IHmrcClient _hmrcClient;
        private ILogger<CheckApprenticeEmploymentStatusQueryHandler> _logger;

        public CheckApprenticeEmploymentStatusQueryHandler(
            IHmrcClient hmrcClient,
            ILogger<CheckApprenticeEmploymentStatusQueryHandler> logger)
        {
            //_employmentCheckClient = employmentCheckClient;
            _hmrcClient = hmrcClient;
            _logger = logger;
        }

        public async Task<CheckApprenticeEmploymentStatusQueryResult> Handle(
            CheckApprenticeEmploymentStatusQueryRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle()";

            ApprenticeEmploymentCheckMessageModel result = null;
            try
            {
                if (request != null &&
                    request.ApprenticeEmploymentCheckMessageModel != null)
                {
                    // Call the application client to store the apprentices employment check queue messages
                    result = await _hmrcClient.CheckApprenticeEmploymentStatus_Client(request.ApprenticeEmploymentCheckMessageModel);
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

            return new CheckApprenticeEmploymentStatusQueryResult(result);
        }
    }
}