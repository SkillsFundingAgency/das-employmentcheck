using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData
{
    public class SubmitLearnerDataClient
        : ISubmitLearnerDataClient
    {
        private ISubmitLearnerDataService _submitLearnerDataService;
        private ILogger<IEmploymentCheckClient> _logger;

        public SubmitLearnerDataClient(
            ISubmitLearnerDataService submitLearnerDataService,
            ILogger<IEmploymentCheckClient> logger)
        {
            _submitLearnerDataService = submitLearnerDataService;
            _logger = logger;
        }

        public async Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(
            IList<EmploymentCheckModel> apprentices)
        {
            var thisMethodName = "SubmitLearnerDataClient.GetApprenticesNiNumber()";

            IList<ApprenticeNiNumber> ApprenticesNiNumber;
            try
            {
                if (apprentices != null)
                {
                    ApprenticesNiNumber = await _submitLearnerDataService.GetApprenticesNiNumber(apprentices);
                    if (ApprenticesNiNumber != null && ApprenticesNiNumber.Count > 0)
                    {
                        _logger.LogInformation($"{thisMethodName}: returned [{ApprenticesNiNumber.Count}] apprentices NI Numbers");
                        //Log.WriteLog(_logger, thisMethodName, $"returned [{ApprenticesNiNumber.Count}] apprentices NI Numbers.");
                    }
                    else
                    {
                        _logger.LogInformation($"{thisMethodName}: returned null/zero apprentices NI Numbers");
                        //Log.WriteLog(_logger, thisMethodName, $"returned null/zero apprentices NI Numbers.");
                        ApprenticesNiNumber = new List<ApprenticeNiNumber>();
                    }
                }
                else
                {
                    _logger.LogInformation("ERROR apprentices parameter is NULL, no employer PAYE schemes retrieved");
                    //Log.WriteLog(_logger, thisMethodName, "ERROR: apprentices parameter is NULL, no employer PAYE schemes retrieved.");
                    ApprenticesNiNumber = new List<ApprenticeNiNumber>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
                ApprenticesNiNumber = new List<ApprenticeNiNumber>();
            }

            return ApprenticesNiNumber;
        }
    }
}
