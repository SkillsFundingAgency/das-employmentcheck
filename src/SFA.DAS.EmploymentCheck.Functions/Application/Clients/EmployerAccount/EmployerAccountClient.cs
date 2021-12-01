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
            IList<Models.Domain.EmploymentCheckModel> employmentCheckModels)
        {
            var thisMethodName = "EmployerAccountClient.GetEmployersPayeSchemes()";

            IList<EmployerPayeSchemes> employerPayeSchemes = new List<EmployerPayeSchemes>();
            try
            {
                if (employmentCheckModels != null && employmentCheckModels.Count != 0)
                {
                    foreach (var employmentCheckModel in employmentCheckModels)
                    {
                        Log.WriteLog(_logger, thisMethodName, $"Getting PAYE scheme for employer account [{employmentCheckModel.AccountId}] (apprentice ULN [{employmentCheckModel.Uln}]).");
                        var accountDetailViewModel = await _employerAccountService.GetEmployerAccount(employmentCheckModel.AccountId);

                        if (accountDetailViewModel != null &&
                            accountDetailViewModel.PayeSchemes != null &&
                            accountDetailViewModel.PayeSchemes.Count > 0)
                        {
                            employerPayeSchemes.Add(new EmployerPayeSchemes(employmentCheckModel.AccountId, accountDetailViewModel.PayeSchemes.Select(x => x.Id).ToList()));
                        }
                        else
                        {
                            _logger.LogInformation($"{thisMethodName}: ERROR: AccountDetailViewModel/PayeSchemes parameter is NULL, no employer PAYE schemes retrieved");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: ERROR: the employmentCheckModels input parameter is NULL, no employer PAYE schemes retrieved");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return employerPayeSchemes;
        }
    }
}
