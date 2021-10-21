using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Clients;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class AccountsService : IAccountsService
    {
        private readonly IAccountsApiClient _accountsApiClient;
        private ILogger<AccountsService> _logger;

        public AccountsService(
            IAccountsApiClient accountsApiClient,
            ILogger<AccountsService> logger)
        {
            _accountsApiClient = accountsApiClient;
        }

        public async Task<AccountDetailViewModel> GetAccountDetail(long accountId)
        {
            var thisMethodName = "AccountsService.GetAccountDetail()";

            AccountDetailViewModel accountDetailViewModel = null;
            try
            {
                accountDetailViewModel = await _accountsApiClient.Get<AccountDetailViewModel>($"api/accounts/internal/{accountId})");
                if(accountDetailViewModel != null && accountDetailViewModel.PayeSchemes != null && accountDetailViewModel.PayeSchemes.Count > 0)
                {
                    Log.WriteLog(_logger, thisMethodName, $"returned {accountDetailViewModel.PayeSchemes.Count} PAYE schemes.");
                }
                {
                    Log.WriteLog(_logger, thisMethodName, $"GetAccountPayeSchemes() returned null/zero PAYE schemes.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\n Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return accountDetailViewModel;
        }
    }
}
