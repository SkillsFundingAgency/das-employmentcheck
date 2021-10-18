using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class LearnersService : ILearnersService
    {
        private readonly ILogger<LearnersService> _logger;

        public LearnersService(
            ILogger<LearnersService> logger)
        {
            _logger = logger;
        }

        public async Task<IList<LearnerNinoDto>> GetLearnersNationalInsuranceNumbers(LearnerNinoDto[] learnersNinosDto)
        {
            var thisMethodName = $"LearnersService.GetLearnersNationalInsuranceNumbers(LearnerNinoDto[] learnersNinosDto)";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            try
            {
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            var result = new List<LearnerNinoDto>();

            return await Task.FromResult(result);
        }
    }
}

//public class AccountsService : IAccountsService
//{
//    private readonly IAccountsApiClient _accountsApiClient;
//    private ILogger<AccountsService> _logger;

//    public AccountsService(
//        IAccountsApiClient accountsApiClient,
//        ILogger<AccountsService> logger)
//    {
//        _accountsApiClient = accountsApiClient;
//    }

//    public async Task<AccountDetailViewModel> GetAccountDetail(long accountId)
//    {
//        var thisMethodName = "AccountsService.GetAccountDetail(long accountId)";
//        var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

//        AccountDetailViewModel account = null;
//        try
//        {
//            _logger.LogInformation($"{messagePrefix} Executing [_accountsApiClient.Get<AccountDetailViewModel>($api/accounts/internal/{accountId})].");

//            account = await _accountsApiClient.Get<AccountDetailViewModel>($"api/accounts/internal/{accountId}");

//            _logger.LogInformation($"{messagePrefix} [_accountsApiClient.Get<AccountDetailViewModel>($api/accounts/internal/{accountId})] returned {account.DasAccountName}.");

//        }
//        catch (Exception ex)
//        {
//            _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
//        }

//        return account;
//    }
//}