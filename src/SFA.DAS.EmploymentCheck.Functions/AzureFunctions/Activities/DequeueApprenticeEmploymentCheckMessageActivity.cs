﻿using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.DequeueApprenticeEmploymentCheckMessage;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class DequeueApprenticeEmploymentCheckMessageActivity
    {
        private const string ThisClassName = "\n\nDequeueApprenticeEmploymentCheckMessagesActivity";
        public const string ErrorMessagePrefix = "[*** ERROR ***]";

        private readonly IMediator _mediator;
        private readonly ILogger<DequeueApprenticeEmploymentCheckMessageActivity> _logger;

        public DequeueApprenticeEmploymentCheckMessageActivity(
            IMediator mediator,
            ILogger<DequeueApprenticeEmploymentCheckMessageActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(DequeueApprenticeEmploymentCheckMessageActivity))]
        public async Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessageActivityTask(
            [ActivityTrigger] object input)
        {
            var thisMethodName = $"{ThisClassName}.DequeueApprenticeEmploymentCheckMessageActivityTask()";

            ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel = null;
            try
            {
                // Send MediatR request to get the next message off the queue
                var dequeueApprenticeEmploymentCheckMessageQueryRequestResult = await _mediator.Send(new DequeueApprenticeEmploymentCheckMessageQueryRequest());

                if (dequeueApprenticeEmploymentCheckMessageQueryRequestResult != null &&
                    dequeueApprenticeEmploymentCheckMessageQueryRequestResult.ApprenticeEmploymentCheckMessage != null)
                {
                    apprenticeEmploymentCheckMessageModel = dequeueApprenticeEmploymentCheckMessageQueryRequestResult.ApprenticeEmploymentCheckMessage;
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: The dequeueApprenticeEmploymentCheckMessageQueryRequestResult value returned from the call to DequeueApprenticeEmploymentCheckMessageQueryRequest() is null.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticeEmploymentCheckMessageModel;
        }
    }
}
