using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.Dispatcher
{
    public class WhenCreatingCommandDispatcherException
    {
        [Test]
        public void Then_Create_CommandDispatcherException_No_Parameters()
        {
            // Arrange and Act
            var commandDispatcherException = new CommandDispatcherException();

            // Assert
            commandDispatcherException.Should().NotBeNull();

        }

        [Test]
        public void Then_Create_CommandDispatcherException_Using_Serialisation()
        {
            // Arrange
            var expectedMessage = "ExceptionTest Message";
            var expectedInnerExceptionMessage = $"Inner Exception Message: {expectedMessage}";
            var commandDispatcherException = new CommandDispatcherException(expectedMessage, new Exception(expectedInnerExceptionMessage));
            CommandDispatcherException actual;

            // Act
            var json = JsonConvert.SerializeObject(commandDispatcherException);
            actual = JsonConvert.DeserializeObject<CommandDispatcherException>(json);

            // Assert
            actual.Should().NotBeNull();
            actual.Message.Should().Be(expectedMessage);
        }
    }
}
