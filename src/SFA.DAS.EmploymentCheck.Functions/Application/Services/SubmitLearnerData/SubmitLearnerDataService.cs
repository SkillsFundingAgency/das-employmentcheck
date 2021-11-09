using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData
{
    public class SubmitLearnerDataService : ISubmitLearnerDataService
    {
        private readonly ILogger<SubmitLearnerDataService> _logger;

        public SubmitLearnerDataService(
            ILogger<SubmitLearnerDataService> logger)
        {
            _logger = logger;
        }

        public async Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(IList<Apprentice> apprentices)
        {
            var thisMethodName = $"SubmitLearnerDataService.GetApprenticeNiNumbers()";

            IList<ApprenticeNiNumber> apprenticeNiNumbers = null;
            try
            {
                // TODO: Implement API call
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(apprenticeNiNumbers);
        }
    }
}