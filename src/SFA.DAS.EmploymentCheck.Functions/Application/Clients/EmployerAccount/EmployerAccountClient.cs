using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount
{
    public class EmployerAccountClient
        : IEmployerAccountClient
    {
        private IEmployerAccountService _employerAccountService;
        private ILogger<IEmploymentCheckClient> _logger;

        public EmployerAccountClient(
            IEmployerAccountService employerAccountService,
            ILogger<IEmploymentCheckClient> logger)
        {
            _employerAccountService = employerAccountService;
            _logger = logger;
        }

        public async Task<IList<EmployerPayeSchemes>> GetEmployersPayeSchemes(
            IList<ApprenticeEmploymentCheckModel> apprentices)
        {
            var thisMethodName = "GetApprenticesNiNumberClient.Get()";

            IList<EmployerPayeSchemes> employerPayeSchemes = new List<EmployerPayeSchemes>();
            try
            {
                if (apprentices != null && apprentices.Count != 0)
                {
                    foreach (var apprentice in apprentices)
                    {
                        Log.WriteLog(_logger, thisMethodName, $"Getting PAYE scheme for employer account [{apprentice.AccountId}] (apprentice ULN [{apprentice.ULN}]).");
                        var accountDetailViewModel = await _employerAccountService.GetEmployerAccount(apprentice.AccountId);

                        if (accountDetailViewModel != null &&
                            accountDetailViewModel.PayeSchemes != null &&
                            accountDetailViewModel.PayeSchemes.Count > 0)
                        {
                            employerPayeSchemes.Add(new EmployerPayeSchemes(apprentice.AccountId, accountDetailViewModel.PayeSchemes.Select(x => x.Id).ToList()));
                        }
                        else
                        {
                            _logger.LogInformation($"{thisMethodName}: ERROR: AccountDetailViewModel/PayeSchemes parameter is NULL, no employer PAYE schemes retrieved");
                            //Log.WriteLog(_logger, thisMethodName, "ERROR: AccountDetailViewModel/PayeSchemes parameter is NULL, no employer PAYE schemes retrieved.");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: ERROR: apprentices parameter is NULL, no employer PAYE schemes retrieved");
                    //Log.WriteLog(_logger, thisMethodName, "ERROR: apprentices parameter is NULL, no employer PAYE schemes retrieved.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employerPayeSchemes;
        }
    }
}
