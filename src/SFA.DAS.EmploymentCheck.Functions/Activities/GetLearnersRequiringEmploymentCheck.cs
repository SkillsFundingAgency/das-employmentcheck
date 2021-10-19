using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetLearnersRequiringEmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetLearnersRequiringEmploymentCheck
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetLearnersRequiringEmploymentCheck> _logger;

        public GetLearnersRequiringEmploymentCheck(
            IMediator mediator,
            ILogger<GetLearnersRequiringEmploymentCheck> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetLearnersRequiringEmploymentCheck))]
        public async Task<List<LearnersRequiringEmploymentCheckDto>> Get([ActivityTrigger] object input)
        {
            var thisMethodName = "Activity: GetLearnersRequiringEmploymentCheck.Get()";

            GetLearnersRequiringEmploymentCheckResult learnersRequiringEmploymentCheckResult = null;
            try
            {
                // Send MediatR request to get the learners for the employment check
                learnersRequiringEmploymentCheckResult = await _mediator.Send(new GetLearnersRequiringEmploymentCheckRequest());
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return learnersRequiringEmploymentCheckResult.LearnersRequiringEmploymentCheckDtos;
        }
    }
}

