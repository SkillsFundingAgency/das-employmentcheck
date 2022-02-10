using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using SFA.DAS.HashingService;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.EmployerAccountsServiceTests
{
    public class WhenGettingEmployerPayeSchemes
    {
        private Fixture _fixture;

        private EmployerAccountApiConfiguration _employerAccountApiConfiguration;
        private Models.EmploymentCheck _employmentCheck;

        private Mock<ILogger<IEmployerAccountService>> _logger;
        private Mock<IHashingService> _hashingService;
        private Mock<IHttpClientFactory> _httpClientFactory;
        private Mock<IWebHostEnvironment> _webHostEnvironment;
        private Mock<IAzureClientCredentialHelper> _azureClientCredentialHelper;
        private Mock<IAccountsResponseRepository> _accountsResponseRepository;


        private IEmployerAccountService _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _employerAccountApiConfiguration = _fixture.Build<EmployerAccountApiConfiguration>()
                .With(c => c.Url, "https://www.microsoft.com").Create();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();

            _logger = new Mock<ILogger<IEmployerAccountService>>();
            _hashingService = new Mock<IHashingService>();
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _webHostEnvironment = new Mock<IWebHostEnvironment>();
            _azureClientCredentialHelper = new Mock<IAzureClientCredentialHelper>();
            _accountsResponseRepository = new Mock<IAccountsResponseRepository>();


            //_httpClientFactory.
            _sut = new EmployerAccountService(
                _logger.Object,
                _employerAccountApiConfiguration,
                _hashingService.Object,
                _httpClientFactory.Object,
                _webHostEnvironment.Object,
                _azureClientCredentialHelper.Object,
                _accountsResponseRepository.Object);
        }

        [Test]
        public async Task The_The_GetEmployerPayeSchemes_Is_Called()
        {
            // Arrange
            _hashingService.Setup(x => x.HashValue(_employmentCheck.AccountId))
                .Returns(_fixture.Create<string>());


            // Act
            await _sut.GetEmployerPayeSchemes(_fixture.Create<Models.EmploymentCheck>());

            // Assert
            _hashingService.Verify(x => x.HashValue(_employmentCheck.AccountId), Times.Exactly(1));
        }
    }
}