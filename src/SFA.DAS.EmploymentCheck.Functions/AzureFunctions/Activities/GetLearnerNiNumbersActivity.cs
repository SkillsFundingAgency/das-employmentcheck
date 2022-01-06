using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetLearnerNiNumbersActivity
    {
        #region Private members
        private const string ThisClassName = "\n\nGetLearnerNiNumbersActivity";
        private const string ErrorMessagePrefix = "[*** ERROR ***]";
        private readonly IMediator _mediator;
        private readonly ILogger<GetLearnerNiNumbersActivity> _logger;
        #endregion Private members

        #region Constructors
        public GetLearnerNiNumbersActivity(
            IMediator mediator,
            ILogger<GetLearnerNiNumbersActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        #endregion Constructors

        #region Get
        [FunctionName(nameof(GetLearnerNiNumbersActivity))]
        public async Task<IList<LearnerNiNumber>> Get([ActivityTrigger] IList<Application.Models.EmploymentCheck> employmentCheckBatch)
        {
            var thisMethodName = $"{ThisClassName}.Get()";
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));

            GetNiNumbersQueryResult getLearnerNiNumbersQueryResult = null;
            try
            {
                getLearnerNiNumbersQueryResult = await _mediator.Send(new GetNiNumbersQueryRequest(employmentCheckBatch));
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return getLearnerNiNumbersQueryResult.LearnerNiNumber;
        }
        #endregion Get
    }
}

