using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesToVerify
{
    public class GetApprenticesToVerifyHandler : IRequestHandler<GetApprenticesToVerifyRequest, GetApprenticesToVerifyResult>
    {
        private IEmploymentCheckService _employmentCheckService;
        private ILoggerAdapter<GetApprenticesToVerifyHandler> _logger;
        private readonly string _connectionString =
            System.Environment.GetEnvironmentVariable($"EmploymentChecksConnectionString");

        public GetApprenticesToVerifyHandler(
            IEmploymentCheckService employmentCheckService,
            ILoggerAdapter<GetApprenticesToVerifyHandler> logger)
        {
            _employmentCheckService = employmentCheckService;
            _logger = logger;
        }

        public async Task<GetApprenticesToVerifyResult> Handle(
            GetApprenticesToVerifyRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = "GetApprenticesToVerifyHandler.Handle()";

            IList<Apprentice> apprentices = null;

            try
            {
                // Call the data repository to get the apprentices to check
                apprentices = await _employmentCheckService.GetApprentices();

                if (apprentices != null && apprentices.Count > 0)
                {
                    Log.WriteLog(_logger, thisMethodName, $"returned {apprentices.Count} learner(s)");
                }
                else
                {
                    Log.WriteLog(_logger, thisMethodName, $"returned null/zero learners.");
                    apprentices = new List<Apprentice>(); // return empty list rather than null
                }
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\n Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetApprenticesToVerifyResult(apprentices);
        }
    }
}
