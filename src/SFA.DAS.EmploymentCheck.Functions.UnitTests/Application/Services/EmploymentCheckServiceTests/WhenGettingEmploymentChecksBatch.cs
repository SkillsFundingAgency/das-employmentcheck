using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.EmploymentCheckServiceTests
{
    public class WhenGettingEmploymentChecksBatch
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
        public async Task Then_GetEmploymentChecksBatch_Is_Called()
        {
            // Arrange
            _employmentCheckRepository.Setup(r => r.GetEmploymentChecksBatch())
                .ReturnsAsync(new List<Models.EmploymentCheck>());

            // Act
            await _sut.GetEmploymentChecksBatch();

            // Assert
            _employmentCheckRepository.Verify(r => r.GetEmploymentChecksBatch(), Times.Exactly(1));
        }

        [Test]
        public async Task Then_If_The_GetEmploymentChecksBatch_Returns_EmploymentChecks_They_Are_Returned()
        {
            // Arrange
            var employmentChecks = _fixture.CreateMany<Models.EmploymentCheck>().ToList();

            _employmentCheckRepository.Setup(r => r.GetEmploymentChecksBatch())
                .ReturnsAsync(employmentChecks);

            // Act
            var result = await _sut.GetEmploymentChecksBatch();

            // Assert
            result.Should().BeEquivalentTo(employmentChecks);
        }

        [Test]
        public async Task Then_If_The_GetEmploymentChecksBatch_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            _employmentCheckRepository.Setup(r => r.GetEmploymentChecksBatch())
                .ReturnsAsync((IList<Models.EmploymentCheck>)null);

            // Act
            var result = await _sut.GetEmploymentChecksBatch();

            // Assert
            result.Should().BeEquivalentTo(new List<Models.EmploymentCheck>());
        }
    }
}