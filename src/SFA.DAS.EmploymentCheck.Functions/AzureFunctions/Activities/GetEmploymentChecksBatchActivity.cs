using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentChecksBatch;

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
            [ActivityTrigger] Object notUsed)
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            GetEmploymentCheckBatchQueryResult getEmploymentCheckBatchResultQuery = null;
            try
            {
                // We may have a partial batch of data so catch any Exceptions in called code and let the Orchestrator check for data
                getEmploymentCheckBatchResultQuery = await _mediator.Send(new GetEmploymentCheckBatchQueryRequest());
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return getEmploymentCheckBatchResultQuery.ApprenticeEmploymentChecks;
        }
        #endregion Get
    }
}