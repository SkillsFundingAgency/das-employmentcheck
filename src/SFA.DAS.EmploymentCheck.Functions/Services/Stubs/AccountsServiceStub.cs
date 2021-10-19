using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Clients;

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
            //var thisMethodName = "***** AccountsService.GetAccountDetail(long accountId) *****";
            //var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

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

            //_logger.LogInformation($"{messagePrefix} ***** STUB Get<AccountDetailViewModel>($api/accounts/internal/{accountId})] returned {accountDetailViewModel.DasAccountName}.");
            return await Task.FromResult(accountDetailViewModel);
        }
    }
}

