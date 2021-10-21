using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetEmployerPayeSchemes
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetEmployerPayeSchemes> _logger;

        public GetEmployerPayeSchemes(
            IMediator mediator,
            ILogger<GetEmployerPayeSchemes> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetEmployerPayeSchemes))]
        public async Task<IList<EmployerPayeSchemesDto>> Get([ActivityTrigger] IList<long> accountIds)
        {
            var thisMethodName = "Activity: GetEmployerPayeSchemes.Get()";

            GetEmployerPayeSchemesResult getEmployerPayeSchemesResult = null;
            try
            {
                if (accountIds != null && accountIds.Count > 0)
                {
                    // Send MediatR request to get the employer paye schemes
                    getEmployerPayeSchemesResult = await _mediator.Send(new GetEmployerPayeSchemesRequest(accountIds));

                    if (getEmployerPayeSchemesResult != null
                        && getEmployerPayeSchemesResult.EmployerPayeSchemesDtos != null
                        && getEmployerPayeSchemesResult.EmployerPayeSchemesDtos.Count > 0)
                    {
                        Log.WriteLog(_logger, thisMethodName, $"returned {getEmployerPayeSchemesResult.EmployerPayeSchemesDtos.Count} PAYE scheme(s)");
                    }
                    else
                    {
                        Log.WriteLog(_logger, thisMethodName, $"*** PARAMETER ACCOUNTIDS IS NULL OR CONTAINS ZERO ENTRIES ***");
                    }
                }
                else
                {
                    Log.WriteLog(_logger, thisMethodName, $"*** RETURNED NULL/ZERO PAYE SCHEME(S) ***");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\n Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return getEmployerPayeSchemesResult.EmployerPayeSchemesDtos;
        }
    }
}
