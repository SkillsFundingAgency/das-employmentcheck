//using System;
//using AutoFixture;
//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Moq;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models;
//using SFA.DAS.EmploymentCheck.Functions.Repositories;
//using SFA.DAS.EmploymentCheck.Functions.Configuration;
//using Microsoft.Azure.Services.AppAuthentication;
//using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
//using static SFA.DAS.EmploymentCheck.Functions.UnitTests.Repositories.AccountsRepositoryTests.WhenSavingResponse;
//using SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureServiceTokenProvider;

//namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Repositories.AccountsRepositoryTests
//{
//    public class WhenSavingResponse
//    {
//        private readonly Mock<IAccountsResponseRepository> _repository;
//        private readonly Mock<ITokenProvider> _tokenProvider;
//        private readonly Mock<TokenProvider> _azureServiceTokenProvider;
//        private readonly Mock<ILogger<IAccountsResponseRepository>> _logger;
//        private readonly Fixture _fixture;
//        private readonly ApplicationSettings _applicationSettings;
//        private readonly AccountsResponse _accountsResponse;

//        public WhenSavingResponse()
//        {
//            _fixture = new Fixture();
//            _repository = new Mock<IAccountsResponseRepository>();
//            _tokenProvider = new Mock<ITokenProvider>();
//            _logger = new Mock<ILogger<IAccountsResponseRepository>>();
//            _applicationSettings = _fixture.Create<ApplicationSettings>();
//            _accountsResponse = _fixture.Create<AccountsResponse>();
//        }

//        [Test]
//        public async Task Then_The_Repository_Is_Called()
//        {
//            Arrange
//            _repository.Setup(x => x.Save(_accountsResponse));
//            var azureServiceTokenProvider = new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider();
//            var token = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com", "1");

//            var sut = new AccountsResponseRepository(_applicationSettings, (Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider)_tokenProvider.Object, _logger.Object);

//            Act
//           await sut.Save(_accountsResponse);

//            Assert
//            _repository.Verify(x => x.Save(_accountsResponse), Times.AtLeastOnce());

//        }

//        [Test]
//        public async Task And_The_EmploymentCheckService_CreateEmploymentCheckCacheRequests_Returns_No_Requests_Then_An_Empty_List_Is_Returned()
//        {
//            //Arrange
//            var employmentCheckData = _fixture.Create<EmploymentCheckData>();

//            _employmentCheckService.Setup(x => x.CreateEmploymentCheckCacheRequests(employmentCheckData))
//                .ReturnsAsync((List<EmploymentCheckCacheRequest>)null);

//            var sut = new EmploymentCheckClient(_logger.Object, _employmentCheckService.Object);

//            //Act
//            var result = await sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

//            //Assert
//            result.Should().BeNull();
//        }

//        [Test]
//        public async Task And_The_EmploymentCheckService_CreateEmploymentCheckCacheRequests_Returns_Null_Then_An_Empty_List_Is_Returned()
//        {
//            //Arrange
//            var employmentCheckData = _fixture.Create<EmploymentCheckData>();

//            _employmentCheckService.Setup(x => x.CreateEmploymentCheckCacheRequests(employmentCheckData))
//                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

//            var sut = new EmploymentCheckClient(_logger.Object, _employmentCheckService.Object);

//            //Act
//            var result = await sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

//            //Assert
//            result.Should().BeEquivalentTo(new List<EmploymentCheckCacheRequest>());
//        }

//        [Test]
//        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
//        {
//            //Arrange
//            var employmentCheckData = _fixture.Create<EmploymentCheckData>();
//            var employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest>
//            {
//                new EmploymentCheckCacheRequest()
//                {
//                    Id = 1,
//                    CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
//                    Nino = "NI12345678",
//                    PayeScheme = "Paye1",
//                    LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
//                    MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
//                    MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
//                    RequestCompletionStatus = 220,
//                    CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
//                }
//            };

//            _employmentCheckService.Setup(x => x.CreateEmploymentCheckCacheRequests(employmentCheckData))
//                .ReturnsAsync(employmentCheckCacheRequests);

//            var sut = new EmploymentCheckClient(_logger.Object, _employmentCheckService.Object);

//            //Act
//            var result = await sut.CreateEmploymentCheckCacheRequests(employmentCheckData);

//            //Assert
//            Assert.AreEqual(employmentCheckCacheRequests, result);
//        }
//    }
//    public interface ITokenProvider
//    {
//        Task<string> GetTokenAsync(string resource);
//    }


//}
