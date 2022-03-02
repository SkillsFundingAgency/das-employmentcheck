using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGettingDbLearnerNiNumbers
    {
        private Fixture _fixture;
        private ILearnerService _sut;

        private Mock<ILogger<ILearnerService>> _loggerMock;
        private Mock<IDcTokenService> _dcTokenServiceMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<ApplicationSettings> _applicationSettingsMock;
        private Mock<IDataCollectionsResponseRepository> _repositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _loggerMock = new Mock<ILogger<ILearnerService>>(MockBehavior.Strict);
            _dcTokenServiceMock = new Mock<IDcTokenService>(MockBehavior.Strict);
            _httpClientFactoryMock = new Mock<IHttpClientFactory>(MockBehavior.Strict);
            _applicationSettingsMock = new Mock<ApplicationSettings>(MockBehavior.Strict);
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>(MockBehavior.Strict);

            var azureTokenServiceProvider = new AzureServiceTokenProvider();
            var services = new ServiceCollection();
            services.AddTransient<IOptions<DcApiSettings>>(provider => Options.Create<DcApiSettings>(new DcApiSettings {}));

            var provider = services.BuildServiceProvider();
            IOptions<DcApiSettings> options = provider.GetService<IOptions<DcApiSettings>>();

            _sut = new LearnerService(
                _loggerMock.Object,
                _dcTokenServiceMock.Object,
                _httpClientFactoryMock.Object,
                options,
                azureTokenServiceProvider,
                _applicationSettingsMock.Object,
                _repositoryMock.Object);
        }

        [Test]
        public async Task Then_GetDbNiNumbers_Is_Called()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            var dataCollectionsResponse = _fixture.Create<DataCollectionsResponse>();
            var learnerNiNumber = new LearnerNiNumber { Uln = employmentCheck.Uln, NiNumber = dataCollectionsResponse.NiNumber };

            _repositoryMock
                .Setup(r => r.GetByEmploymentCheckId(It.IsAny<long>()))
                .ReturnsAsync(dataCollectionsResponse);

            // Act
            var actual = await _sut.GetDbNiNumber(employmentCheck);

            // Assert
            actual.Should().BeEquivalentTo(learnerNiNumber);
        }
    }

    public class Option
    {
        private readonly DcApiSettings _settings;

        public Option(IOptions<DcApiSettings> settings)
        {
            _settings = settings.Value;
        }

        public string Name => _settings.BaseUrl;
    }
}