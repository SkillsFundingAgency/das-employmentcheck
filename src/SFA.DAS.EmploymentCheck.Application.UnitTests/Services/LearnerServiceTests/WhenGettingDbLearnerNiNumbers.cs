using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Net;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.LearnerServiceTests
{
    public class WhenGettingDbLearnerNiNumbers
    {
        private ILearnerService _sut;
        private Fixture _fixture;
        private Mock<IDataCollectionsResponseRepository> _repositoryMock;
        private Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>> _apiClientMock;
        private Mock<IApiOptionsRepository> _apiOptionsRepositoryMock;
        private ApiRetryOptions _settings;
        private DataCollectionsApiConfiguration _apiConfiguration;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _apiClientMock = new Mock<IDataCollectionsApiClient<DataCollectionsApiConfiguration>>();
            _repositoryMock = new Mock<IDataCollectionsResponseRepository>(MockBehavior.Strict);
            _apiConfiguration = new DataCollectionsApiConfiguration();

            _apiOptionsRepositoryMock = new Mock<IApiOptionsRepository>();

            _settings = new ApiRetryOptions
            {
                TooManyRequestsRetryCount = 10,
                TransientErrorRetryCount = 2,
                TransientErrorDelayInMs = 1
            };

            _apiOptionsRepositoryMock.Setup(r => r.GetOptions(It.IsAny<string>())).ReturnsAsync(_settings);

            var retryPolicies = new ApiRetryPolicies(
                Mock.Of<ILogger<ApiRetryPolicies>>(),
                _apiOptionsRepositoryMock.Object);

            _sut = new LearnerService(
                _apiClientMock.Object,
                _repositoryMock.Object,
                retryPolicies,
                Mock.Of<ILogger<LearnerService>>(),
                _apiConfiguration);
        }

        [Test]
        public async Task Then_GetDbNiNumbers_Is_Called()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            var dataCollectionsResponse = _fixture.Build<DataCollectionsResponse>().With(_ => _.Uln, employmentCheck.Uln).With(_ => _.HttpStatusCode, (short)HttpStatusCode.OK).Create();

            _repositoryMock
                .Setup(r => r.GetByEmploymentCheckId(It.Is<long>(x => x == employmentCheck.Id)))
                .ReturnsAsync(dataCollectionsResponse);

            // Act
            var actual = await _sut.GetDbNiNumber(employmentCheck);

            // Assert
            actual.Uln.Should().Be(employmentCheck.Uln);
            actual.NiNumber.Should().Be(dataCollectionsResponse.NiNumber);
            actual.HttpStatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}