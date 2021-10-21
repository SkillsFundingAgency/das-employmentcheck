using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Clients;
using SFA.DAS.EmploymentCheck.Functions.Helpers;

namespace SFA.DAS.EmploymentCheck.Functions.Services.Stubs
{
    public class AccountsServiceStub : IAccountsService
    {
        private readonly IAccountsApiClient _accountsApiClient;
        private readonly ILogger<IAccountsService> _logger;

        public AccountsServiceStub(
            IAccountsApiClient accountsApiClient,
            ILogger<IAccountsService> logger)
        {
            _accountsApiClient = accountsApiClient;
            _logger = logger;
        }

        public async Task<AccountDetailViewModel> GetAccountDetail(long accountId)
        {
            var thisMethodName = "STUB AccountsService.GetAccountDetail(): ";

            var payeSchemesResourceViewModel = new ResourceViewModel();
            payeSchemesResourceViewModel.Id = "1";
            payeSchemesResourceViewModel.Href = "";

            var paySchemesResourceViewModelList = new List<ResourceViewModel>();
            paySchemesResourceViewModelList.Add(payeSchemesResourceViewModel);

            var accountDetailViewModel = new AccountDetailViewModel
            {
                AccountId = 1,
                StartingTransferAllowance = 10m,
                RemainingTransferAllowance = 5m,
                PayeSchemes = new ResourceList(paySchemesResourceViewModelList)
            };

            Log.WriteLog(_logger, thisMethodName, $"returned {accountDetailViewModel.PayeSchemes.Count} PAYE schemes.");
            return await Task.FromResult(accountDetailViewModel);
        }
    }
}

