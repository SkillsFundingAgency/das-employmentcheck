using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureServiceTokenProvider;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Repositories.AccountsRepositoryTests
{
    public class WhenSavingResponse
    {
        private readonly Mock<IAccountsResponseRepository> _repository;
        private readonly Mock<ITokenProvider> _tokenProvider;
        private readonly Mock<TokenProvider> _azureServiceTokenProvider;
        private readonly Mock<ILogger<IAccountsResponseRepository>> _logger;
        private readonly Fixture _fixture;
        private readonly ApplicationSettings _applicationSettings;
        private readonly AccountsResponse _accountsResponse;

        public WhenSavingResponse()
        {
            _fixture = new Fixture();
            _repository = new Mock<IAccountsResponseRepository>();
            _tokenProvider = new Mock<ITokenProvider>();
            _logger = new Mock<ILogger<IAccountsResponseRepository>>();
            _applicationSettings = _fixture.Build<ApplicationSettings>().With(x => x.DbConnectionString, "Server=.;Database=SFA.DAS.EmploymentCheck.Database;Trusted_Connection=True;").Create<ApplicationSettings>();
            _accountsResponse = _fixture.Create<AccountsResponse>();
        }

        [Test]
        public async Task Then_The_Repository_Is_Called()
        {
            // TODO:

            // Arrange
            //_repository.Setup(x => x.Save(_accountsResponse))
            //    .Returns(Task.FromResult(0));

            //var sut = new AccountsResponseRepository(_applicationSettings, null, _logger.Object);

            ////_repository.

            //// Act
            //await sut.Save(_accountsResponse);

            //// Assert
            //_repository.Verify(x => x.Save(_accountsResponse), Times.AtLeastOnce());
        }
    }
}