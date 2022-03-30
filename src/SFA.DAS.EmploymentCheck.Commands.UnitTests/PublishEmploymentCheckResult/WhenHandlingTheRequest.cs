using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using SFA.DAS.EmploymentCheck.Types;
using SFA.DAS.NServiceBus.Services;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.PublishEmploymentCheckResult
{
    public class WhenHandlingTheRequest
    {
        private PublishEmploymentCheckResultCommandHandler _sut;
        private Mock<IEventPublisher> _mockEventPublisher;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mockEventPublisher = new Mock<IEventPublisher>();

            _sut = new PublishEmploymentCheckResultCommandHandler(_mockEventPublisher.Object, Mock.Of<ILogger<PublishEmploymentCheckResultCommandHandler>>());
        }

        [Test]
        public async Task Then_a_message_is_published()
        {
            // Arrange
            var request = _fixture.Create<PublishEmploymentCheckResultCommand>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _mockEventPublisher.Verify(
                _ => _.Publish(
                    It.Is<EmploymentCheckCompletedEvent>(c => 
                        c.CorrelationId == request.EmploymentCheck.CorrelationId
                        && c.CheckDate == request.EmploymentCheck.LastUpdatedOn
                        && c.EmploymentResult == request.EmploymentCheck.Employed
                        && c.ErrorType == request.EmploymentCheck.ErrorType)
                    ), Times.Once);
        }
    }
}
