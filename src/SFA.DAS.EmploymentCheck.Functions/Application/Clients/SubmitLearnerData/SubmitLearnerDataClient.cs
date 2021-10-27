﻿using Microsoft.Extensions.Logging;
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
        private ILoggerAdapter<IEmploymentCheckClient> _logger;

        public SubmitLearnerDataClient(
            ISubmitLearnerDataService submitLearnerDataService,
            ILoggerAdapter<IEmploymentCheckClient> logger)
        {
            _submitLearnerDataService = submitLearnerDataService;
            _logger = logger;
        }

        public async Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(
            IList<Apprentice> apprentices)
        {
            var thisMethodName = "SubmitLearnerDataClient.GetApprenticesNiNumber()";

            IList<ApprenticeNiNumber> ApprenticesNiNumber = null;
            try
            {
                if (apprentices != null)
                {
                    ApprenticesNiNumber = await _submitLearnerDataService.GetApprenticesNiNumber(apprentices);
                    if (ApprenticesNiNumber != null && ApprenticesNiNumber.Count > 0)
                    {
                        Log.WriteLog(_logger, thisMethodName, $"returned [{ApprenticesNiNumber.Count}] apprentices NI Numbers.");
                    }
                    else
                    {
                        Log.WriteLog(_logger, thisMethodName, $"returned null/zero apprentices NI Numbers.");
                    }
                }
                else
                {
                    Log.WriteLog(_logger, thisMethodName, "ERROR: apprentices parameter is NULL, no employer PAYE schemes retrieved.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return ApprenticesNiNumber;
        }
    }
}
