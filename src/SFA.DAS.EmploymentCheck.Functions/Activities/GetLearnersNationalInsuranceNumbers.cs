using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetLearnersNationalInsuranceNumbers
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetLearnersNationalInsuranceNumbers> _logger;

        public GetLearnersNationalInsuranceNumbers(
            IMediator mediator,
            ILogger<GetLearnersNationalInsuranceNumbers> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetLearnersNationalInsuranceNumbers))]
        public async Task Verify([ActivityTrigger] ApprenticeToVerifyDto apprentice)
        {
            var thisMethodName = "*** CheckApprentice.Verify([ActivityTrigger] ApprenticeToVerifyDto apprentice) activity";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}

