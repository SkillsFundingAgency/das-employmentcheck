using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner
{
    public class LearnerClient
        : ILearnerClient
    {
        #region Private members
        private readonly ILogger<IEmploymentCheckClient> _logger;
        private readonly ILearnerService _learnerService;
        #endregion Private members

        #region Constructors
        public LearnerClient(
            ILogger<IEmploymentCheckClient> logger,
            ILearnerService learnerService)
        {
            _logger = logger;
            _learnerService = learnerService;
        }
        #endregion Constructors

        #region GetNiNumbers
        public async Task<IList<LearnerNiNumber>> GetNiNumbers(
            IList<Models.EmploymentCheck> employmentCheckBatch)
        {
            var thisMethodName = MethodBase.GetCurrentMethod().Name;

            // Exceptions as a result of input data are 'thrown' back to the caller
            // See: https://awesomeopensource.com/project/ardalis/GuardClauses
            Guard.Against.NullOrEmpty(employmentCheckBatch, nameof(employmentCheckBatch));

            // Exceptions in called code are caught here
            IList<LearnerNiNumber> learnerNiNumbers = null;
            try
            {
                learnerNiNumbers = await _learnerService.GetNiNumbers(employmentCheckBatch);
                if (learnerNiNumbers != null &&
                    learnerNiNumbers.Count > 0)
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: returned [{learnerNiNumbers.Count}] learner NI Numbers");
                }
                else
                {
                    _logger.LogInformation($"\n\n{thisMethodName}: returned null/zero learner NI Numbers");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return learnerNiNumbers;
        }
        #endregion GetNiNumbers
    }
}