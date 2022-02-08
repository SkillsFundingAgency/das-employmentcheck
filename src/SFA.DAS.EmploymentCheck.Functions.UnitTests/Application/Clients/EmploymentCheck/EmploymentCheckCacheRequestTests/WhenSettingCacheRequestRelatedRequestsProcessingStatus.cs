using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckCacheRequestTests
{
    public class WhenSettingCacheRequestRelatedRequestsProcessingStatus
    {
        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;
        private readonly Fixture _fixture;
        private readonly IList<EmploymentCheckCacheRequest> _employmentCheckCacheRequests;
        private readonly Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> _status;

        public WhenSettingCacheRequestRelatedRequestsProcessingStatus()
        {
            _fixture = new Fixture();
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
            _employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest> { _fixture.Create<EmploymentCheckCacheRequest>()};
            _status = new Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>(_fixture.Create<EmploymentCheckCacheRequest>(), _fixture.Create<ProcessingCompletionStatus>());
        }

        [Test]
        public async Task Then_The_EmploymentCheckService_Is_Called()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.SetCacheRequestRelatedRequestsProcessingStatus(_status))
                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

            var sut = new EmploymentCheckClient(_logger.Object, _employmentCheckService.Object);

            //Act
            await sut.SetCacheRequestRelatedRequestsProcessingStatus(_status);

            //Assert
            _employmentCheckService.Verify(x => x.SetCacheRequestRelatedRequestsProcessingStatus(_status), Times.AtLeastOnce());
        }

        [Test]
        public async Task And_The_EmploymentCheckService_CreateEmploymentCheckCacheRequests_Returns_No_Requests_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.SetCacheRequestRelatedRequestsProcessingStatus(_status))
                .ReturnsAsync((List<EmploymentCheckCacheRequest>)null);


            var sut = new EmploymentCheckClient(_logger.Object, _employmentCheckService.Object);

            //Act
            var result = await sut.SetCacheRequestRelatedRequestsProcessingStatus(_status);

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_EmploymentCheckService_CreateEmploymentCheckCacheRequests_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.SetCacheRequestRelatedRequestsProcessingStatus(_status))
                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

            var sut = new EmploymentCheckClient(_logger.Object, _employmentCheckService.Object);

            //Act
            var result = await sut.SetCacheRequestRelatedRequestsProcessingStatus(_status);

            //Assert
            result.Should().BeEquivalentTo(new List<EmploymentCheckCacheRequest>());
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.SetCacheRequestRelatedRequestsProcessingStatus(_status))
                .ReturnsAsync(_employmentCheckCacheRequests);

            var sut = new EmploymentCheckClient(_logger.Object, _employmentCheckService.Object);

            //Act
            var result = await sut.SetCacheRequestRelatedRequestsProcessingStatus(_status);

            //Assert
            Assert.AreEqual(_employmentCheckCacheRequests, result);
        }
    }
}