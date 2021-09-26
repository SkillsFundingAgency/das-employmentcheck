using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Queries.GetApprenticesToVerify;

namespace SFA.DAS.EmploymentCheck.Functions.Activities
{
    public class GetApprenticesToCheck
    {
        private readonly IMediator _mediator;
        private readonly ILogger<GetApprenticesToCheck> _logger;

        public GetApprenticesToCheck(IMediator mediator, ILogger<GetApprenticesToCheck> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [FunctionName(nameof(GetApprenticesToCheck))]
        public async Task<List<ApprenticeToVerifyDto>> Get([ActivityTrigger] object input)
        {
            _logger.LogInformation("Getting apprentices to check.");
            var apprenticesToCheck = await _mediator.Send(new GetApprenticesToVerifyRequest());
            var apprenticesToCheckCount = apprenticesToCheck.ApprenticesToVerify.Count;
            _logger.LogInformation("{payableLegalEntitiesCount} apprentices to check.", apprenticesToCheckCount);

            return apprenticesToCheck.ApprenticesToVerify;
        }
    }
}
