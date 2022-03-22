using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.NServiceBus.Services;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.Publisher
{
    public class WhenSendIsCalled
    {
        private CommandPublisher _sut;
        private Mock<IEventPublisher> _mockEventPublisher;
        private Fixture _fixture;

        public class TestCommand : ICommand { }

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockEventPublisher = new Mock<IEventPublisher>();

            _sut = new CommandPublisher(_mockEventPublisher.Object);
        }

        [Test]
        public async Task Then_the_command_is_published_to_servicebus()
        {
            // Arrange
            var command = _fixture.Create<TestCommand>();

            // Act
            await _sut.Publish(command);

            // Assert
            _mockEventPublisher.Verify(m => m.Publish(command), Times.Once);
        }
    }
}
