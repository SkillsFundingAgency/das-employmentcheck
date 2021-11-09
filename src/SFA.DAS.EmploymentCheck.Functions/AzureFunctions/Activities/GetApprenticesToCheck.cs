using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesToVerify;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
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
        public async Task<IList<Apprentice>> Get([ActivityTrigger] object input)
        {
            var thisMethodName = "GetApprenticesToCheck.Get()";

            GetApprenticesToVerifyResult apprenticesToCheck = null;
            try
            {
                apprenticesToCheck = await _mediator.Send(new GetApprenticesToVerifyRequest());
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return apprenticesToCheck.ApprenticesToVerify;
        }
    }
}
