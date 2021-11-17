using System;
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
        private const string ThisClassName = "\n\nGetApprenticesNiNumberActivity";
        private readonly IMediator _mediator;
        private readonly ILogger<GetApprenticesNiNumberActivity> _logger;

        public GetApprenticesNiNumberActivity(
            IMediator mediator,
            ILogger<GetApprenticesNiNumberActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetApprenticesNiNumberActivity))]
        public async Task<IList<ApprenticeNiNumber>> Get([ActivityTrigger] IList<ApprenticeEmploymentCheckModel> apprentices)
        {
            var thisMethodName = $"{ThisClassName}.Get()";

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

