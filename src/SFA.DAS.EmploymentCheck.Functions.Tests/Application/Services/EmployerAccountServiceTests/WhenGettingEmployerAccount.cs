//using System;
//using System.Collections.Generic;
//using Microsoft.Extensions.Logging;
//using Moq;
//using SFA.DAS.EAS.Account.Api.Types;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
//using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
//using SFA.DAS.EmploymentCheck.Functions.Helpers;
//using Xunit;

//namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Services.EmployerAccountServiceTests
//{
//    public class WhenGettingEmployerAccount
//    {
//        private readonly Mock<IEmployerAccountApiClient> _accountsApiClient;
//        private readonly Mock<ILogger<IEmployerAccountService>> _logger;
//        private readonly long _accountId;

//        public WhenGettingEmployerAccount()
//        {
//            _accountsApiClient = new Mock<IEmployerAccountApiClient>();
//            _logger = new Mock<ILogger<IEmployerAccountService>>();
//            _accountId = 1;
//        }

//        [Fact]
//        public async void Then_The_EmployerAccountApiClient_Is_Called()
//        {
//            //Arrange

//            _accountsApiClient.Setup(x => x.Get<AccountDetailViewModel>(It.IsAny<string>()))
//                .ReturnsAsync(new AccountDetailViewModel());

//            var sut = new EmployerAccountService(_accountsApiClient.Object, _logger.Object);

//            //Act

//            await sut.GetEmployerAccount(_accountId);

//            //Assert

//            _accountsApiClient.Verify(x => x.Get<AccountDetailViewModel>($"api/accounts/internal/{_accountId})"), Times.Exactly(1));
//        }

//        [Fact]
//        public async void And_The_EmployerAccountApiClient_Returns_An_Account_Then_It_Is_Returned()
//        {
//            //Arrange

//            var payeScheme = new ResourceViewModel();
//            payeScheme.Href = "href";
//            payeScheme.Id = "id";
//            var payeSchemes = new List<ResourceViewModel> {payeScheme};
//            var returnPayeSchemes = new ResourceList(payeSchemes);

//            var account = new AccountDetailViewModel {PayeSchemes = returnPayeSchemes};

//            _accountsApiClient.Setup(x => x.Get<AccountDetailViewModel>($"api/accounts/internal/{_accountId})"))
//                .ReturnsAsync(account);

//            var sut = new EmployerAccountService(_accountsApiClient.Object, _logger.Object);

//            //Act

//            var result = await sut.GetEmployerAccount(_accountId);

//            //Assert
            
//            Assert.Equal(account, result);
//        }

//        [Fact]
//        public async void And_The_EmployerAccountApiClient_Returns_Null_Then_Null_Is_Returned()
//        {
//            //Arrange

//            _accountsApiClient.Setup(x => x.Get<AccountDetailViewModel>(It.IsAny<string>()))
//                .ReturnsAsync((AccountDetailViewModel) null);

//            var sut = new EmployerAccountService(_accountsApiClient.Object, _logger.Object);

//            //Act

//            var result = await sut.GetEmployerAccount(_accountId);

//            //Assert

//            Assert.Null(result);
//        }

//        [Fact]
//        public async void And_The_EmployerAccountApiClient_Returns_Null_PayeSchemes_Then_AccountDetailViewModel_Is_Returned()
//        {
//            //Arrange

//            var account = new AccountDetailViewModel {PayeSchemes = null};

//            _accountsApiClient.Setup(x => x.Get<AccountDetailViewModel>(It.IsAny<string>()))
//                .ReturnsAsync(account);

//            var sut = new EmployerAccountService(_accountsApiClient.Object, _logger.Object);

//            //Act

//            var result = await sut.GetEmployerAccount(_accountId);

//            //Assert

//            Assert.Equal(account, result);
//        }

//        [Fact]
//        public async void And_The_EmployerAccountApiClient_Returns_Zero_PayeSchemes_Then_The_AccountDetailViewModel_Is_Returned()
//        {
//            //Arrange

//            var account = new AccountDetailViewModel { PayeSchemes = new ResourceList(new List<ResourceViewModel>()) };

//            _accountsApiClient.Setup(x => x.Get<AccountDetailViewModel>(It.IsAny<string>()))
//                .ReturnsAsync(account);

//            var sut = new EmployerAccountService(_accountsApiClient.Object, _logger.Object);

//            //Act

//            var result = await sut.GetEmployerAccount(_accountId);

//            //Assert

//            Assert.Equal(account, result);
//        }

//        [Fact]
//        public async void
//            And_The_EmployerAccountApiClient_Throws_An_Exception_Then_Null_Is_Returned()
//        {
//            //Arrange

//            var exception = new Exception("exception");

//            _accountsApiClient.Setup(x => x.Get<AccountDetailViewModel>(It.IsAny<string>())).ThrowsAsync(exception);

//            var sut = new EmployerAccountService(_accountsApiClient.Object, _logger.Object);

//            //Act

//            var result = await sut.GetEmployerAccount(_accountId);

//            //Assert

//            Assert.Null(result);
//        }
//    }
//}