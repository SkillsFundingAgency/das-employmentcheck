using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetLearnersNationalInsuranceNumbers;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetLearnersNationalInsuranceNumbersActivity
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetLearnersNationalInsuranceNumbersActivity> _logger;

        public GetLearnersNationalInsuranceNumbersActivity(
            IMediator mediator,
            ILogger<GetLearnersNationalInsuranceNumbersActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetLearnersNationalInsuranceNumbersActivity))]
        public async Task<List<LearnerNationalnsuranceNumberDto>> Get([ActivityTrigger] object input)
        {
            var thisMethodName = "***** Activity: GetLearnersNationalInsuranceNumbers.Get()";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            GetLearnersNationalInsuranceNumbersResult learnerNationalnsuranceNumberResult = null;
            try
            {
                // Send MediatR request to get the apprentices for the employment check
                learnerNationalnsuranceNumberResult = await _mediator.Send(new GetLearnersNationalInsuranceNumbersRequest());
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return learnerNationalnsuranceNumberResult.LearnerNationalnsuranceNumberDtos;
        }
    }
}

