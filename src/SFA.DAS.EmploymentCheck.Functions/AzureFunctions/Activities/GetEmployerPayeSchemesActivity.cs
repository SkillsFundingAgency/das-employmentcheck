using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmployerPayeSchemesActivity
    {
        #region Private members
        private const string ThisClassName = "\n\nGetEmployerPayeSchemesActivity";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        private readonly IMediator _mediator;
        private readonly ILogger<GetEmployerPayeSchemesActivity> _logger;
        #endregion Private members

        #region Constructors
        public GetEmployerPayeSchemesActivity(
            IMediator mediator,
            ILogger<GetEmployerPayeSchemesActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        #endregion Constructors

        #region Get
        [FunctionName(nameof(Activities.GetEmployerPayeSchemesActivity))]
        public async Task<IList<EmployerPayeSchemes>> Get([ActivityTrigger] IList<Application.Models.EmploymentCheck> employmentCheckBatch)
        {
            var thisMethodName = $"{ThisClassName}.Get()";
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));

            GetPayeSchemesQueryResult getPayeSchemesQueryResult = null;
            try
            {
                getPayeSchemesQueryResult = await _mediator.Send(new GetPayeSchemesQueryRequest(employmentCheckBatch));
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return getPayeSchemesQueryResult.EmployersPayeSchemes;
        }
        #endregion Get
    }
}
