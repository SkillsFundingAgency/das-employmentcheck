using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using SFA.DAS.EmploymentCheck.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.PublishEmploymentCheckResult
{
    public class WhenHandlingTheRequest
    {
        private PublishEmploymentCheckResultCommandHandler _sut;
        private Mock<IMessageSession> _messageSessionMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _messageSessionMock = new Mock<IMessageSession>();
            var lazyMessageSession = new Lazy<IMessageSession>(() => _messageSessionMock.Object);

            _sut = new PublishEmploymentCheckResultCommandHandler(
                lazyMessageSession,
                Mock.Of<ILogger<PublishEmploymentCheckResultCommandHandler>>());
        }

        [Test]
        public async Task Then_a_message_is_published()
        {
            // Arrange
            var request = _fixture.Create<PublishEmploymentCheckResultCommand>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _messageSessionMock.Verify(
                _ => _.Publish(
                    It.Is<EmploymentCheckCompletedEvent>(c => 
                        c.CorrelationId == request.EmploymentCheck.CorrelationId
                        && c.CheckDate == request.EmploymentCheck.LastUpdatedOn
                        && c.EmploymentResult == request.EmploymentCheck.Employed
                        && c.ErrorType == request.EmploymentCheck.ErrorType),
                    It.IsAny<PublishOptions>()
                    ), Times.Once);
        }
    }
}
