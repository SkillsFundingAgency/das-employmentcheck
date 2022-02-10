using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.EmploymentCheckTests
{
    public class WhenGettingEmploymentCheckCacheRequest
    {
        private Fixture _fixture;

        private Mock<ILogger<IEmploymentCheckService>> _logger;
        private Mock<IEmploymentCheckRepository> _employmentCheckRepository;
        private Mock<IEmploymentCheckCacheRequestRepository> _employmentCheckCashRequestRepository;
        private IEmploymentCheckService _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _logger = new Mock<ILogger<IEmploymentCheckService>>();
            _employmentCheckRepository = new Mock<IEmploymentCheckRepository>();
            _employmentCheckCashRequestRepository = new Mock<IEmploymentCheckCacheRequestRepository>();

            _sut = new EmploymentCheckService(
                _logger.Object,
                _employmentCheckRepository.Object,
                _employmentCheckCashRequestRepository.Object);
        }

        [Test]
        public async Task Then_GetEmploymentCheckCacheRequest_Is_Called()
        {
            // Arrange
            _employmentCheckCashRequestRepository.Setup(r => r.GetNext())
                .ReturnsAsync(new EmploymentCheckCacheRequest());

            // Act
            await _sut.GetEmploymentCheckCacheRequest();

            // Assert
            _employmentCheckCashRequestRepository.Verify(r => r.GetNext(), Times.Exactly(1));
        }

        [Test]
        public async Task Then_If_The_GetEmploymentCheckCacheRequest_Returns_EmploymentCheckCacheRequests_They_Are_Returned()
        {
            // Arrange
            var employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();

            _employmentCheckCashRequestRepository.Setup(r => r.GetNext())
                .ReturnsAsync(employmentCheckCacheRequest);

            // Act
            var result = await _sut.GetEmploymentCheckCacheRequest();

            // Assert
            result.Should().BeEquivalentTo(employmentCheckCacheRequest);
        }

        [Test]
        public async Task Then_If_The_GetEmploymentCheckCacheRequest_Returns_Null_Then_Null_Is_Returned()
        {
            // Arrange
            _employmentCheckCashRequestRepository.Setup(r => r.GetNext())
                .ReturnsAsync((EmploymentCheckCacheRequest)null);

            // Act
            var result = await _sut.GetEmploymentCheckCacheRequest();

            // Assert
            result.Should().BeNull();
        }
    }
}