﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using AgileObjects.AgileMapper;
using AgileObjects.AgileMapper.Extensions;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmployersPayeSchemesActivity
    {
        private readonly IMediator _mediator;
        private readonly ILoggerAdapter<GetEmployersPayeSchemesActivity> _logger;

        public GetEmployersPayeSchemesActivity(
            IMediator mediator,
            ILoggerAdapter<GetEmployersPayeSchemesActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(Activities.GetEmployersPayeSchemesActivity))]
        public async Task<IList<EmployerPayeSchemes>> Get([ActivityTrigger] IList<Apprentice> apprentices)
        {
            var thisMethodName = "GetEmployersPayeSchemesActivity.Get()";

            GetEmployersPayeSchemesMediatorResult getEmployerPayeSchemesResult = null;
            try
            {
                getEmployerPayeSchemesResult = await _mediator.Send(new GetEmployersPayeSchemesMediatorRequest(apprentices));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");

                getEmployerPayeSchemesResult =
                    new GetEmployersPayeSchemesMediatorResult(new List<EmployerPayeSchemes>()); //returns new list instead of null
            }

            return getEmployerPayeSchemesResult.EmployersPayeSchemes;
        }
    }
}