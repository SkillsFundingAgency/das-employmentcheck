using AutoFixture;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using SFA.DAS.HashingService;
using System.Net.Http;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

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




        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _logger = new Mock<ILogger<IEmployerAccountService>>(MockBehavior.Strict);

            _employerAccountApiConfiguration = _fixture.Build<EmployerAccountApiConfiguration>()
                .With(c => c.Url, "https://www.microsoft.com").Create();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();

            _hashingService = new Mock<IHashingService>(MockBehavior.Strict);
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _webHostEnvironment = new Mock<IWebHostEnvironment>(MockBehavior.Strict);
            _azureClientCredentialHelper = new Mock<IAzureClientCredentialHelper>(MockBehavior.Strict);
            _accountsResponseRepository = new Mock<IAccountsResponseRepository>(MockBehavior.Strict);
        }

        [Test]
        public async Task The_The_GetEmployerPayeSchemes_Is_Called()
        {
            // Arrange
            _hashingService.Setup(x => x.HashValue("PROPER VALUE"))
                .Returns("ANOTHER PROPER VALUE" );

            //var clientHandlerStub = new DelegatingHandlerStub();
            //var client = new HttpClient(clientHandlerStub);

            //_httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            //IHttpClientFactory factory = mockFactory.Object;

            _httpClientFactory.Setup(f => f.CreateClient("Name"))
                .Returns(new HttpClient());


            var sut = new EmployerAccountService(
                _logger.Object,
                _employerAccountApiConfiguration,
                _hashingService.Object,
                _httpClientFactory.Object,
                _webHostEnvironment.Object,
                _azureClientCredentialHelper.Object,
                _accountsResponseRepository.Object);

            // Act
            //await _sut.GetEmployerPayeSchemes(_fixture.Create<Models.EmploymentCheck>());

              // await sut.GetPayeSchemesFromApiResponse(_employmentCheck, )

            // Assert
            _hashingService.Verify(x => x.HashValue(_employmentCheck.AccountId), Times.Exactly(1));

            // Checks
            // GetPayeSchemesFromApiResponse
            // if (httpResponseMessage == null)

            // accountsResponse.HttpResponse = httpResponseMessage.ToString();
            // accountsResponse.HttpStatusCode = (short)httpResponseMessage.StatusCode;

            // if (!httpResponseMessage.IsSuccessStatusCode)
        }
    }
}