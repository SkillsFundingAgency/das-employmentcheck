using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.EmploymentCheckTests
{
    public class WhenSettingCacheRequestRelatedRequestsProcessingStatus
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
        public async Task Then_SetCacheRequestRelatedRequestsProcessingStatus_Is_Called()
        {
            // Arrange
            var status = _fixture.Create<Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>>();

            _employmentCheckCashRequestRepository.Setup(r => r.SetRelatedRequestsCompletionStatus(status))
                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

            // Act
            await _sut.SetCacheRequestRelatedRequestsProcessingStatus(status);

            // Assert
            _employmentCheckCashRequestRepository.Verify(r => r.SetRelatedRequestsCompletionStatus(status), Times.Exactly(1));
        }

        [Test]
        public async Task Then_If_SetCacheRequestRelatedRequestsProcessingStatus_Returns_EmploymentCheckCacheRequests_They_Are_Returned()
        {
            // Arrange
            var status = _fixture.Create<Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>>();
            var employmentCheckCacheRequests = _fixture.CreateMany<EmploymentCheckCacheRequest>().ToList();

            _employmentCheckCashRequestRepository.Setup(r => r.SetRelatedRequestsCompletionStatus(status))
                .ReturnsAsync(employmentCheckCacheRequests);

            // Act
            var result = await _sut.SetCacheRequestRelatedRequestsProcessingStatus(status);

            // Assert
            result.Should().BeEquivalentTo(employmentCheckCacheRequests);
        }

        [Test]
        public async Task Then_If_SetCacheRequestRelatedRequestsProcessingStatus_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            var status = _fixture.Create<Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>>();

            _employmentCheckCashRequestRepository.Setup(r => r.SetRelatedRequestsCompletionStatus(status))
                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

            // Act
            var result = await _sut.SetCacheRequestRelatedRequestsProcessingStatus(status);

            // Assert
            result.Should().BeEquivalentTo(new List<EmploymentCheckCacheRequest>());
        }
    }
}