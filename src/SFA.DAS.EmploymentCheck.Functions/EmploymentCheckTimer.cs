using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Queries.GetApprenticesToVerify;

namespace SFA.DAS.EmploymentCheck.Functions
{
    public class EmploymentCheckTimer
    {
        private readonly IMediator _mediator;

        public EmploymentCheckTimer(IMediator mediator)
        {
            _mediator = mediator;
        }

        [FunctionName(nameof(EmploymentCheckTimer))]
        public async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            var queryResult = await _mediator.Send(new GetApprenticesToVerifyRequest());
            foreach (var result in queryResult.ApprenticesToVerify)
            {
                log.LogInformation("ULN: " + result.ULN);
            }
            
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
