﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Linq;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetApprenticesNiNumberActivity
    {
        private readonly IMediator _mediator;
        private readonly ILoggerAdapter<GetApprenticesNiNumberActivity> _logger;

        public GetApprenticesNiNumberActivity(
            IMediator mediator,
            ILoggerAdapter<GetApprenticesNiNumberActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetApprenticesNiNumberActivity))]
        public async Task<IList<ApprenticeNiNumber>> Get([ActivityTrigger] IList<Apprentice> apprentices)
        {
            var thisMethodName = "GetApprenticesNiNumbersActivity.Get()";

            GetApprenticesNiNumberMediatorResult getApprenticesNiNumberResult;
            try
            {
                getApprenticesNiNumberResult = await _mediator.Send(new GetApprenticesNiNumberMediatorRequest(apprentices));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                getApprenticesNiNumberResult = new GetApprenticesNiNumberMediatorResult(new List<ApprenticeNiNumber>()); //returns empty list instead of null
            }

            return getApprenticesNiNumberResult.ApprenticesNiNumber;
        }
    }
}
