using AutoFixture;
using Moq;
using NServiceBus;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using ICommand = SFA.DAS.EmploymentCheck.Abstractions.ICommand;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.Publisher
{
    public class WhenSendIsCalled
    {
        private CommandPublisher _sut;
        private Mock<IMessageSession> _mockEventPublisher;
        private Fixture _fixture;

        public class TestCommand : ICommand { }

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockEventPublisher = new Mock<IMessageSession>();

            _sut = new CommandPublisher(new Lazy<IMessageSession>(() => _mockEventPublisher.Object));
        }

        [Test]
        public async Task Then_the_command_is_published_to_servicebus()
        {
            // Arrange
            var command = _fixture.Create<TestCommand>();

            // Act
            await _sut.Publish(command);

            // Assert
            _mockEventPublisher.Verify(m => m.Send(command, It.IsAny<SendOptions>()), Times.Once);
        }
    }
}
