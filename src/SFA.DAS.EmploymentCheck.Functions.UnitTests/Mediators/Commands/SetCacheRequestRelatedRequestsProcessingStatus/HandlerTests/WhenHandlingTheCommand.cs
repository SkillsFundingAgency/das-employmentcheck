using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.SetCacheRequestRelatedRequestsProcessingStatus.HandlerTests
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<IEmploymentCheckClient> _employmentCheckClient;
        private EmploymentCheckData _employmentCheckData;
        private IList<EmploymentCheckCacheRequest> _employmentCheckCacheRequests;
        private Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus> _requestStatus;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheckClient = new Mock<IEmploymentCheckClient>();
            _employmentCheckData = _fixture.Create<EmploymentCheckData>();
            _employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest> { _fixture.Create<EmploymentCheckCacheRequest>() };
            _requestStatus = new Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>(_fixture.Create<EmploymentCheckCacheRequest>(), _fixture.Create<ProcessingCompletionStatus>());
        }

        [Test]
        public async Task Then_The_EmploymentCheckClient_Is_Called()
        {
            // Arrange
            var command = new SetCacheRequestRelatedRequestsProcessingStatusCommand(_requestStatus);

            _employmentCheckClient.Setup(x => x.SetCacheRequestRelatedRequestsProcessingStatus(command.EmploymentCheckCacheRequestAndStatusToSet))
                .ReturnsAsync(new List<EmploymentCheckCacheRequest>());

            var sut = new SetCacheRequestRelatedRequestsProcessingStatusCommandHandler(_employmentCheckClient.Object);

            // Act
            await sut.Handle(new SetCacheRequestRelatedRequestsProcessingStatusCommand(_requestStatus), CancellationToken.None);

            // Assert
            _employmentCheckClient.Verify(x => x.SetCacheRequestRelatedRequestsProcessingStatus(command.EmploymentCheckCacheRequestAndStatusToSet), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_Null_Then_Null_Is_Returned()
        {
            // Arrange
            var command = new SetCacheRequestRelatedRequestsProcessingStatusCommand(_requestStatus);

            _employmentCheckClient.Setup(x => x.SetCacheRequestRelatedRequestsProcessingStatus(command.EmploymentCheckCacheRequestAndStatusToSet))
                .ReturnsAsync((IList<EmploymentCheckCacheRequest>)null);

            var sut = new SetCacheRequestRelatedRequestsProcessingStatusCommandHandler(_employmentCheckClient.Object);

            // Act
            var result = await sut.Handle(new SetCacheRequestRelatedRequestsProcessingStatusCommand(_requestStatus), CancellationToken.None);

            // Assert
            result.EmploymentCheckCacheRequests.Should().BeNull();
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_EmploymentCheckCacheRequests_Then_They_Are_Returned()
        {
            // Arrange
            var command = new SetCacheRequestRelatedRequestsProcessingStatusCommand(_requestStatus);

            _employmentCheckClient.Setup(x => x.SetCacheRequestRelatedRequestsProcessingStatus(command.EmploymentCheckCacheRequestAndStatusToSet))
                .ReturnsAsync(_employmentCheckCacheRequests);

            var sut = new SetCacheRequestRelatedRequestsProcessingStatusCommandHandler(_employmentCheckClient.Object);

            // Act
            var result = await sut.Handle(new SetCacheRequestRelatedRequestsProcessingStatusCommand(_requestStatus), CancellationToken.None);

            // Assert
            result.EmploymentCheckCacheRequests.Should().BeEquivalentTo(_employmentCheckCacheRequests);
        }
    }
}