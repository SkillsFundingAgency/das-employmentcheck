﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus
{
    public class CheckApprenticeEmploymentStatusQueryHandler
        : IRequestHandler<CheckApprenticeEmploymentStatusQueryRequest,
            CheckApprenticeEmploymentStatusQueryResult>
    {
        private const string ThisClassName = "\n\nCheckApprenticeEmploymentStatusCommandHandler";

        //private IEmploymentCheckClient _employmentCheckClient;
        private IHmrcClient _hmrcClient;
        private ILogger<CheckApprenticeEmploymentStatusQueryHandler> _logger;

        public CheckApprenticeEmploymentStatusQueryHandler(
            //IEmploymentCheckClient employmentCheckClient,
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

            ApprenticeEmploymentCheckMessageModel ApprenticeEmploymentCheckMessageModelResult = null;
            try
            {
                if (request != null &&
                    request.ApprenticeEmploymentCheckMessageModel != null)
                {
                    // Call the application client to store the apprentices employment check queue messages
                    ApprenticeEmploymentCheckMessageModelResult = await _hmrcClient.CheckApprenticeEmploymentStatus_Client(request.ApprenticeEmploymentCheckMessageModel);
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

            return new CheckApprenticeEmploymentStatusQueryResult(ApprenticeEmploymentCheckMessageModelResult);
        }
    }
}
