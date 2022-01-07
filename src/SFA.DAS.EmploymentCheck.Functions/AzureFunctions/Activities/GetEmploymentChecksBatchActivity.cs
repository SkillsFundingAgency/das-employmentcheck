using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentChecksBatchActivity
    {
        #region Private members
        private readonly ILogger<GetEmploymentChecksBatchActivity> _logger;
        private readonly IMediator _mediator;
        #endregion Private members

        #region Constructors
        public GetEmploymentChecksBatchActivity(
            ILogger<GetEmploymentChecksBatchActivity> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }
        #endregion Constructors

        #region Get

        [FunctionName(nameof(GetEmploymentChecksBatchActivity))]
        public async Task<IList<Application.Models.EmploymentCheck>> Get(
            [ActivityTrigger] object _)
        {
            var result = await _mediator.Send(new GetEmploymentCheckBatchQueryRequest());

            return result.ApprenticeEmploymentChecks;
        }

        #endregion Get
    }
}