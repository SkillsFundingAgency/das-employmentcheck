using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.HashingService;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Services.EmployerAccountServiceTests
{
    public class WhenGettingEmployerAccount
    {
        private readonly Mock<IEmployerAccountApiClient> _accountsApiClient;
        private readonly Mock<ILogger<IEmployerAccountService>> _logger;
        private readonly Mock<IHashingService> _hashingService;
        private readonly long _accountId;
        private readonly Fixture _fixture;
        private readonly string _hashedAccountId;

        public WhenGettingEmployerAccount()
        {
            _fixture = new Fixture();
            _accountsApiClient = new Mock<IEmployerAccountApiClient>();
            _logger = new Mock<ILogger<IEmployerAccountService>>();
            _hashingService = new Mock<IHashingService>();
            _accountId = _fixture.Create<long>();
            _hashedAccountId = _fixture.Create<string>();
            _hashingService.Setup(hs => hs.HashValue(_accountId)).Returns(_hashedAccountId);
        }

        [Fact]
        public async Task Then_The_EmployerAccountApiClient_Is_Called()
        {
            // Arrange
            var accounts = _fixture.Create<ResourceList>();
            _accountsApiClient.Setup(x => x.Get<ResourceList>(It.IsAny<string>()))
                .ReturnsAsync(accounts);

            var sut = new EmployerAccountService(_accountsApiClient.Object, _hashingService.Object, _logger.Object);

            // Act
            await sut.GetEmployerAccount(_accountId);

            // Assert
            _accountsApiClient.Verify(x => x.Get<ResourceList>($"api/accounts/{_hashedAccountId}/payeschemes"), Times.Exactly(1));
        }

        [Fact]
        public async Task And_The_EmployerAccountApiClient_Returns_An_Account_Then_It_Is_Returned()
        {
            // Arrange
            var payeScheme = new ResourceViewModel {Href = "href", Id = "id"};
            var account = new ResourceList(new [] {payeScheme});

            _accountsApiClient.Setup(x => x.Get<ResourceList>($"api/accounts/{_hashedAccountId}/payeschemes"))
                .ReturnsAsync(account);

            var sut = new EmployerAccountService(_accountsApiClient.Object, _hashingService.Object, _logger.Object);

            // Act
            var result = await sut.GetEmployerAccount(_accountId);

            // Assert
            Assert.Equal(account, result);
        }

        [Fact]
        public async Task And_The_EmployerAccountApiClient_Returns_Null_Then_Null_Is_Returned()
        {
            //Arrange

            _accountsApiClient.Setup(x => x.Get<ResourceList>(It.IsAny<string>()))
                .ReturnsAsync((ResourceList)null);

            var sut = new EmployerAccountService(_accountsApiClient.Object, _hashingService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployerAccount(_accountId);

            //Assert

            Assert.Null(result);
        }

        [Fact]
        public async Task And_The_EmployerAccountApiClient_Returns_Null_PayeSchemes_Then_AccountDetailViewModel_Is_Returned()
        {
            // Arrange
            var account = (ResourceList) null;

            _accountsApiClient.Setup(x => x.Get<ResourceList>(It.IsAny<string>()))
                .ReturnsAsync(account);

            var sut = new EmployerAccountService(_accountsApiClient.Object, _hashingService.Object, _logger.Object);

            // Act

            var result = await sut.GetEmployerAccount(_accountId);

            // Assert
            Assert.Equal(account, result);
        }

        [Fact]
        public async Task And_The_EmployerAccountApiClient_Returns_Zero_PayeSchemes_Then_The_AccountDetailViewModel_Is_Returned()
        {
            // Arrange
            var account = _fixture.Create<ResourceList>(); 

            _accountsApiClient.Setup(x => x.Get<ResourceList>(It.IsAny<string>()))
                .ReturnsAsync(account);

            var sut = new EmployerAccountService(_accountsApiClient.Object, _hashingService.Object, _logger.Object);

            // Act
            var result = await sut.GetEmployerAccount(_accountId);

            // Assert
            Assert.Equal(account, result);
        }

        [Fact]
        public async Task And_The_EmployerAccountApiClient_Throws_An_Exception_Then_Null_Is_Returned()
        {
            // Arrange
            var exception = new Exception("exception");
            _accountsApiClient.Setup(x => x.Get<ResourceList>(It.IsAny<string>())).ThrowsAsync(exception);
            var sut = new EmployerAccountService(_accountsApiClient.Object, _hashingService.Object, _logger.Object);

            // Act
            var result = await sut.GetEmployerAccount(_accountId);

            // Assert
            Assert.Null(result);
        }
    }
}