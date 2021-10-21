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
using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.Extensions;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmployerPayeSchemesActivity
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetEmployerPayeSchemesActivity> _logger;

        public GetEmployerPayeSchemesActivity(
            IMediator mediator,
            ILogger<GetEmployerPayeSchemesActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(Activities.GetEmployerPayeSchemesActivity))]
        public async Task<IList<EmployerPayeSchemesDto>> Get([ActivityTrigger] IList<LearnerRequiringEmploymentCheckDto> learnerRequiringEmploymentCheckDtos)
        {
            var thisMethodName = "GetEmployerPayeSchemesActivity.Get()";

            IList<EmployerPayeSchemesDto> employerPayeSchemesDtos = null;
            try
            {
                // map the list of learners input param to the required type for the employer paye scheme parameter
                employerPayeSchemesDtos = learnerRequiringEmploymentCheckDtos.Map().ToANew<List<EmployerPayeSchemesDto>>();

                // make the call to get the paye schemes
                var result =  await GetEmployerPayeSchemes(employerPayeSchemesDtos);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\nException caught - {ex.Message}. {ex.StackTrace}");
            }

            return employerPayeSchemesDtos;
        }

        private async Task<IList<EmployerPayeSchemesDto>> GetEmployerPayeSchemes(IList<EmployerPayeSchemesDto> employerPayeSchemesDtos)
        {
            var thisMethodName = "GetEmployerPayeSchemesActivity.GetEmployerPayeSchemes()";

            GetEmployerPayeSchemesResult getEmployerPayeSchemesResult = null;
            try
            {
                if (employerPayeSchemesDtos != null && employerPayeSchemesDtos.Count > 0)
                {
                    // Send MediatR request to get the employer paye schemes
                    getEmployerPayeSchemesResult = await _mediator.Send(new GetEmployerPayeSchemesRequest(employerPayeSchemesDtos));

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
