using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes
{
    public class GetEmployerPayeSchemesMediatorHandler
        : IRequestHandler<GetEmployersPayeSchemesMediatorRequest,
            GetEmployersPayeSchemesMediatorResult>
    {
        private readonly IEmployerAccountClient _employerAccountClient;
        private ILoggerAdapter<GetEmployerPayeSchemesMediatorHandler> _logger;

        public GetEmployerPayeSchemesMediatorHandler(
            IEmployerAccountClient employerAccountClient,
            ILoggerAdapter<GetEmployerPayeSchemesMediatorHandler> logger)
        {
            _employerAccountClient = employerAccountClient;
            _logger = logger;
        }

        public async Task<GetEmployersPayeSchemesMediatorResult> Handle(
            GetEmployersPayeSchemesMediatorRequest getEmployersPayeSchemesMediatorRequest,
            CancellationToken cancellationToken)
        {
            var thisMethodName = "GetEmployerPayeSchemesMediatorHandler.Handle()";

            IList<EmployerPayeSchemes> employersPayeSchemes = null;
            try
            {
                if (getEmployersPayeSchemesMediatorRequest.Apprentices != null &&
                    getEmployersPayeSchemesMediatorRequest.Apprentices.Count > 0)
                {
                    // Call the application client to get the employer PAYE schemes for the apprentices
                    employersPayeSchemes = await _employerAccountClient.GetEmployersPayeSchemes(getEmployersPayeSchemesMediatorRequest.Apprentices);

                    if (employersPayeSchemes != null && employersPayeSchemes.Count > 0)
                    {
                        _logger.LogInformation($"{thisMethodName} returned {employersPayeSchemes.Count} PAYE scheme(s)");
                        //Log.WriteLog(_logger, thisMethodName, $"returned {employersPayeSchemes.Count} PAYE scheme(s)");
                    }
                    else
                    {
                        _logger.LogInformation($"{thisMethodName} returned null/zero PAYE schemes");
                        //Log.WriteLog(_logger, thisMethodName, $"returned null/zero PAYE schemes.");
                        employersPayeSchemes = new List<EmployerPayeSchemes>(); // return empty list rather than null
                    }
                }
                else
                {
                    _logger.LogInformation("ERROR - apprentices parameter is null, no employer PAYE schemes were retrieved");
                    //Log.WriteLog(_logger, thisMethodName, $"ERROR - apprentices parameter is null, no employer PAYE schemes were retrieved.");
                    employersPayeSchemes = new List<EmployerPayeSchemes>(); // return empty list rather than null
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetEmployersPayeSchemesMediatorResult(employersPayeSchemes);
        }
    }
}