using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.Dispatcher
{
    public class WhenSendIsCalled
    {
        private CommandDispatcher _sut;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<ICommandHandler<TestCommand>> _mockTestCommandHandler;
        private Mock<ICommandHandler<TestCommand2>> _mockTestCommand2Handler;
        private Mock<ICommandHandler<TestCommand3>> _mockTestCommand3Handler;

        private Fixture _fixture;

        public class TestCommand : ICommand { }
        public class TestCommand2: ICommand { }
        public class TestCommand3 : ICommand { }
        public class TestCommand4 : ICommand { }

        [SetUp]
        public void Arrange()
        {
            _fixture = new Fixture();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockTestCommandHandler = new Mock<ICommandHandler<TestCommand>>();
            _mockTestCommand2Handler = new Mock<ICommandHandler<TestCommand2>>();
            _mockTestCommand3Handler = new Mock<ICommandHandler<TestCommand3>>();

            _mockServiceProvider.Setup(m => m.GetService(typeof(ICommandHandler<TestCommand2>)))
                .Returns(_mockTestCommand2Handler.Object);
            _mockServiceProvider.Setup(m => m.GetService(typeof(ICommandHandler<TestCommand>)))
                .Returns(_mockTestCommandHandler.Object);
            _mockServiceProvider.Setup(m => m.GetService(typeof(ICommandHandler<TestCommand3>)))
                .Returns(_mockTestCommand3Handler.Object);

            _sut = new CommandDispatcher(_mockServiceProvider.Object);
        }

        [Test]
        public async Task Then_the_matching_command_handler_is_retrived_from_the_service_provider()
        {
            //Arrange
            var command = _fixture.Create<TestCommand>();

            //Act
            await _sut.Send(command);

            //Assert
            _mockServiceProvider.Verify(m => m.GetService(typeof(ICommandHandler<TestCommand>)), Times.Once);
        }

        [Test]
        public async Task Then_the_command_is_handled_by_the_matching_handler()
        {
            //Arrange
            var command = _fixture.Create<TestCommand>();
            bool isHandled = false;
            var mockHandler = new Mock<ICommandHandler<TestCommand>>();

            _mockServiceProvider.Setup(m => m.GetService(typeof(ICommandHandler<TestCommand>)))
               .Returns(mockHandler.Object);

            mockHandler.Setup(m => m.Handle(command, It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    isHandled = true;
                });

            //Act
            await _sut.Send(command);

            //Assert
            isHandled.Should().BeTrue();
        }

        [Test]
        public void Then_a_CommandDispatcherException_is_raised_if_there_is_no_matching_handler()
        {
            // Arrange
            var command = _fixture.Create<TestCommand4>();

            // Act
            Action action = () => _sut.Send(command);

            // Assert
            action.Should().Throw<CommandDispatcherException>().Where(e => e.Message.StartsWith($"Unable to dispatch command '{command.GetType().Name}'. No matching handler found."));
        }
    }
}
