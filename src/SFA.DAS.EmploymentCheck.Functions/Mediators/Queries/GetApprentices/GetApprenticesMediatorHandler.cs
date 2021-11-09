using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprentices
{
    public class GetApprenticesMediatorHandler
        : IRequestHandler<GetApprenticesMediatorRequest,
            GetApprenticesMediatorResult>
    {
        private IEmploymentCheckClient _employmentCheckClient;
        private ILogger<GetApprenticesMediatorHandler> _logger;

        public GetApprenticesMediatorHandler(
            IEmploymentCheckClient employmentCheckClient,
            ILogger<GetApprenticesMediatorHandler> logger)
        {
            _employmentCheckClient = employmentCheckClient;
            _logger = logger;
        }

        public async Task<GetApprenticesMediatorResult> Handle(
            GetApprenticesMediatorRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = "GetApprenticesMediatorHandler.Handle()";

            IList<Apprentice> apprentices = null;
            try
            {
                // Call the application client to get the apprentices requiring an employment check
                apprentices = await _employmentCheckClient.GetApprentices();

                if (apprentices != null && apprentices.Count > 0)
                {
                    _logger.LogInformation($"{thisMethodName} returned {apprentices.Count} apprentice(s)");
                    //Log.WriteLog(_logger, thisMethodName, $"returned {apprentices.Count} apprentice(s)");
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName} returned null/zero apprentices");
                    //Log.WriteLog(_logger, thisMethodName, $"returned null/zero apprentices.");
                    apprentices = new List<Apprentice>(); // return empty list rather than null
                }
            }
            catch (Exception ex)    
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetApprenticesMediatorResult(apprentices);
        }
    }
}
