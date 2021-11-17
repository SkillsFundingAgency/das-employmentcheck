using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.CheckApprenticeEmploymentStatus;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CheckApprenticeEmploymentStatusActivity
    {
        private const string ThisClassName = "\n\nCheckApprenticeEmploymentStatusActivity";

        private readonly IMediator _mediator;
        private readonly ILogger<SaveApprenticeEmploymentCheckResultActivity> _logger;

        public CheckApprenticeEmploymentStatusActivity(
            IMediator mediator,
            ILogger<SaveApprenticeEmploymentCheckResultActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(CheckApprenticeEmploymentStatusActivity))]
        public async Task CheckApprenticeEmploymentStatusActivityTask(
            [ActivityTrigger] ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessage)
        {
            var thisMethodName = $"{ThisClassName}.CheckApprenticeEmploymentStatusActivityTask()";

            try
            {
                // Send MediatR request to check the apprentices employment status using the HMRC API
                await _mediator.Send(new CheckApprenticeEmploymentStatusQueryRequest(apprenticeEmploymentCheckMessage));
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
