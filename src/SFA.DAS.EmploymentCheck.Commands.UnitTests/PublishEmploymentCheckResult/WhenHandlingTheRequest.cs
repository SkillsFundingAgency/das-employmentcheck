using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using SFA.DAS.EmploymentCheck.Commands.Types;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.PublishEmploymentCheckResult
{
    public class WhenHandlingTheRequest
    {
        private EmploymentCheckCompletedEventHandler _sut;
        private Mock<ICommandPublisher> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<ICommandPublisher>();
            _sut = new EmploymentCheckCompletedEventHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_a_message_is_published()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCompletedEvent>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(
                _ => _.Publish(
                    It.Is<PublishEmploymentCheckResultCommand>(c => 
                        c.CorrelationId == request.EmploymentCheck.CorrelationId
                        && c.CheckDate == request.EmploymentCheck.LastUpdatedOn
                        && c.EmploymentResult == request.EmploymentCheck.Employed
                        && c.ErrorType == request.EmploymentCheck.ErrorType),
                    CancellationToken.None), Times.Once);
        }
    }
}
