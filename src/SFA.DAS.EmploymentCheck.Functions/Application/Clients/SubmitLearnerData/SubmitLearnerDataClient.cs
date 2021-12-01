using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData
{
    public class SubmitLearnerDataClient
        : ISubmitLearnerDataClient
    {
        private readonly ISubmitLearnerDataService _submitLearnerDataService;
        private readonly ILogger<IEmploymentCheckClient> _logger;

        public SubmitLearnerDataClient(
            ISubmitLearnerDataService submitLearnerDataService,
            ILogger<IEmploymentCheckClient> logger)
        {
            _submitLearnerDataService = submitLearnerDataService;
            _logger = logger;
        }

        public async Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(
            IList<ApprenticeEmploymentCheckModel> apprentices)
        {
            const string thisMethodName = "SubmitLearnerDataClient.GetApprenticesNiNumber()";

            IList<ApprenticeNiNumber> apprenticesNiNumber;
            try
            {
                if (apprentices != null)
                {
                    apprenticesNiNumber = await _submitLearnerDataService.GetApprenticesNiNumber(apprentices);
                    if (apprenticesNiNumber != null && apprenticesNiNumber.Count > 0)
                    {
                        _logger.LogInformation($"\n\n{thisMethodName}: returned [{apprenticesNiNumber.Count}] apprentices NI Numbers");
                    }
                    else
                    {
                        _logger.LogInformation($"\n\n{thisMethodName}: returned null/zero apprentices NI Numbers");
                        apprenticesNiNumber = new List<ApprenticeNiNumber>();
                    }
                }
                else
                {
                    _logger.LogError($"\n\n{thisMethodName}: ** ERROR ** apprentices parameter is NULL, no employer PAYE schemes retrieved");
                    apprenticesNiNumber = new List<ApprenticeNiNumber>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                apprenticesNiNumber = new List<ApprenticeNiNumber>();
            }

            return apprenticesNiNumber;
        }
    }
}
