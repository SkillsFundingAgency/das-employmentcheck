using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetHmrcLearnerEmploymentStatus
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<IHmrcClient> _hmrcClient;
        private Mock<ILogger<GetHmrcLearnerEmploymentStatusQueryHandler>> _logger;
        private EmploymentCheckCacheRequest _employmentCheckCacheRequest;
        private IList<Models.EmploymentCheck> _employmentChecks;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _hmrcClient = new Mock<IHmrcClient>();
            _logger = new Mock<ILogger<GetHmrcLearnerEmploymentStatusQueryHandler>>();
            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            _employmentChecks = new List<Models.EmploymentCheck> { _fixture.Create<Models.EmploymentCheck>() };
        }

        [Test]
        public async Task Then_The_EmploymentCheckClient_Is_Called()
        {
            // Arrange
            _hmrcClient.Setup(x => x.CheckEmploymentStatus(It.IsAny<EmploymentCheckCacheRequest>()))
                .ReturnsAsync(new EmploymentCheckCacheRequest());

            var sut = new GetHmrcLearnerEmploymentStatusQueryHandler(_hmrcClient.Object, _logger.Object);

            // Act
            await sut.Handle(new GetHmrcLearnerEmploymentStatusQueryRequest(new EmploymentCheckCacheRequest()), CancellationToken.None);

            // Assert
            _hmrcClient.Verify(x => x.CheckEmploymentStatus(It.IsAny<EmploymentCheckCacheRequest>()), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_Null_Then_Null_Is_Returned()
        {
            // Arrange
            _hmrcClient.Setup(x => x.CheckEmploymentStatus(It.IsAny<EmploymentCheckCacheRequest>()))
                .ReturnsAsync((EmploymentCheckCacheRequest)null);

            var sut = new GetHmrcLearnerEmploymentStatusQueryHandler(_hmrcClient.Object, _logger.Object);

            // Act
            var result = await sut.Handle(new GetHmrcLearnerEmploymentStatusQueryRequest(new EmploymentCheckCacheRequest()), CancellationToken.None);

            // Assert
            result.EmploymentCheckCacheRequest.Should().BeNull();
        }

        [Test]
        public async Task And_The_EmploymentCheckClient_Returns_EmploymentChecks_Then_They_Are_Returned()
        {
            // Arrange
            _hmrcClient.Setup(x => x.CheckEmploymentStatus(It.IsAny<EmploymentCheckCacheRequest>()))
                .ReturnsAsync(_employmentCheckCacheRequest);

            var sut = new GetHmrcLearnerEmploymentStatusQueryHandler(_hmrcClient.Object, _logger.Object);

            // Act
            var result = await sut.Handle(new GetHmrcLearnerEmploymentStatusQueryRequest(new EmploymentCheckCacheRequest()), CancellationToken.None);

            // Assert
            result.EmploymentCheckCacheRequest.Should().BeEquivalentTo(_employmentCheckCacheRequest);
        }
    }
}
