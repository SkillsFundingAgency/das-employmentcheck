using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.EmploymentCheckServiceTests
{
    public class WhenUpdatingEmploymentCheck
    {
        private Fixture _fixture;
        private Mock<ILogger<IEmploymentCheckService>> _logger;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCashRequestRepositoryMock = new Mock<IEmploymentCheckCacheRequestRepository>();

        private ApplicationSettings _applicationSettings;
        private IEmploymentCheckService _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _applicationSettings = _fixture.Build<ApplicationSettings>().With(x => x.DbConnectionString, "Server=.;Database=SFA.DAS.EmploymentCheck.Database;Trusted_Connection=True;").Create<ApplicationSettings>();
            _employmentCheckRepositoryMock = new Mock<IEmploymentCheckRepository>();
            _employmentCheckCashRequestRepositoryMock = new Mock<IEmploymentCheckCacheRequestRepository>();
        }

        [Test]
        public async Task Then_UpdateEmploymentCheck_Is_Called()
        {
            // Arrange
            _logger = new Mock<ILogger<IEmploymentCheckService>>();
            var employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();

            _sut = new EmploymentCheckService(
                _logger.Object,
                _applicationSettings,
                null,
                _employmentCheckRepositoryMock.Object,
                _employmentCheckCashRequestRepositoryMock.Object);

            // Act
            await _sut
                .UpdateEmploymentCheck(employmentCheckCacheRequest);

            // Assert
            // no exception is thrown
        }
    }
}