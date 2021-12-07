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
            IList<EmploymentCheckModel> employmentCheckModels)
        {
            var thisMethodName = $"{nameof(EmployerAccountClient)}.GetEmployersPayeSchemes()";

            IList<EmployerPayeSchemes> employerPayeSchemes = new List<EmployerPayeSchemes>();
            try
            {
                if (employmentCheckModels != null && employmentCheckModels.Count != 0)
                {
                    foreach (var employmentCheckModel in employmentCheckModels)
                    {
                        Log.WriteLog(_logger, thisMethodName, $"Getting PAYE scheme for employer account [{employmentCheckModel.AccountId}] (apprentice ULN [{employmentCheckModel.Uln}]).");
                        var resourceList = await _employerAccountService.GetAccountPayeSchemes(employmentCheckModel.AccountId);

                        if (resourceList != null && resourceList.Any())
                        {
                            employerPayeSchemes.Add(new EmployerPayeSchemes(employmentCheckModel.AccountId, resourceList.Select(x => x.Id).ToList()));
                        }
                        else
                        {
                            _logger.LogInformation($"{thisMethodName}: ERROR: resourceList parameter is NULL, no employer PAYE schemes retrieved");
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"{thisMethodName}: ERROR: the resourceList input parameter is NULL, no employer PAYE schemes retrieved");
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
