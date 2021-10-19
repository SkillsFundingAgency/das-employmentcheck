using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesToVerify;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetApprenticesToCheck
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetApprenticesToCheck> _logger;

        public GetApprenticesToCheck(
            IMediator mediator,
            ILogger<GetApprenticesToCheck> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetApprenticesToCheck))]
        public async Task<List<ApprenticeToVerifyDto>> Get([ActivityTrigger] object input)
        {
            var thisMethodName = "***** Activity: GetApprenticesToCheck.Get() *****";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            GetApprenticesToVerifyResult apprenticesToCheck = null;
            try
            {
                // Send MediatR request to get the apprentices for the employment check
                apprenticesToCheck = await _mediator.Send(new GetApprenticesToVerifyRequest());

                //if(apprenticesToCheck != null && apprenticesToCheck != null && apprenticesToCheck.ApprenticesToVerify.Count > 0)
                //{
                //    _logger.LogInformation($"{messagePrefix} ***** MediatR request GetApprenticesToVerifyRequest() returned {apprenticesToCheck.ApprenticesToVerify.Count} apprentices. *****");
                //}
                //else
                //{
                //    _logger.LogInformation($"{messagePrefix} ***** MediatR request [GetApprenticesToVerifyRequest()] returned null or zero apprentices. *****");
                //}
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} ***** Exception caught - {ex.Message}. {ex.StackTrace} *****");
            }

            return apprenticesToCheck.ApprenticesToVerify;
        }
    }
}
