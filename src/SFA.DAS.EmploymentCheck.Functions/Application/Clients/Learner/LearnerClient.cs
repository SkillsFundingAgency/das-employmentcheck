using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;
using System.Collections.Generic;
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

        public async Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Models.EmploymentCheck> apprentices)
        {
            Guard.Against.NullOrEmpty(apprentices, nameof(apprentices));

            var learnerNiNumbers = await _learnerService.GetNiNumbers(apprentices);

            return learnerNiNumbers;
        }

        #endregion GetNiNumbers
    }
}