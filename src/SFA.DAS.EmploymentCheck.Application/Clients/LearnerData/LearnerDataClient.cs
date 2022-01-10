using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Interfaces.LearnerData;
using SFA.DAS.EmploymentCheck.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Clients.LearnerData
{
    public class LearnerDataClient
        : ILearnerDataClient
    {
        #region Private members
        private readonly ILogger<ILearnerDataClient> _logger;
        private readonly ILearnerDataService _learnerService;
        #endregion Private members

        #region Constructors
        public LearnerDataClient(
            ILogger<ILearnerDataClient> logger,
            ILearnerDataService learnerService)
        {
            _logger = logger;
            _learnerService = learnerService;
        }
        #endregion Constructors

        #region GetNiNumbers
        public async Task<IList<LearnerNiNumber>> GetNiNumbers(
            IList<Domain.Entities.EmploymentCheck> employmentCheckBatch)
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