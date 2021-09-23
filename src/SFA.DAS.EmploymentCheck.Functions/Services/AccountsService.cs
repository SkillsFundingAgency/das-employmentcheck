using System;
using System.Threading.Tasks;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Clients;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public class AccountsService : IAccountsService
    {
        private readonly IAccountsApiClient _accountsApiClient;

        public AccountsService(IAccountsApiClient accountsApiClient)
        {
            _accountsApiClient = accountsApiClient;
        }

        public async Task<AccountDetailViewModel> GetAccountDetail(string hashedAccountId)
        {
            return await _accountsApiClient.Get<AccountDetailViewModel>($"api/accounts/{hashedAccountId}");
        }
    }
}
