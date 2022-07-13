using FluentAssertions;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
        public void Then_Create_CommandDispatcherException_With_Message_And_InnerExcption()
        {
            // Arrange
            var expectedMessage = "ExceptionTest Message";
            var expectedInnerExceptionMessage = $"Inner Exception Message: {expectedMessage}";
            

            // Act
            var commandDispatcherException = new CommandDispatcherException(expectedMessage, new Exception(expectedInnerExceptionMessage));

            // Assert
            commandDispatcherException.Should().NotBeNull();
            commandDispatcherException.Message.Should().Be(expectedMessage);
            commandDispatcherException.InnerException.Message.Should().Be(expectedInnerExceptionMessage);   

        }

        [Test]
        public void Then_Create_CommandDispatcherException_Using_Serailisation()
        {
            // Arrange
            var expectedMessage = "ExceptionTest Message";
            var expectedInnerExceptionMessage = $"Inner Exception Message: {expectedMessage}";
            var commandDispatcherException = new CommandDispatcherException(expectedMessage, new Exception(expectedInnerExceptionMessage));
            CommandDispatcherException actual;

            // Act
            using (MemoryStream mem = new MemoryStream())
            {
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(mem, commandDispatcherException);
                mem.Seek(0, SeekOrigin.Begin);
                actual = b.Deserialize(mem) as CommandDispatcherException;
            }

            // Assert
            actual.Should().NotBeNull();
            actual.Message.Should().Be(expectedMessage);
            actual.InnerException.Message.Should().Be(expectedInnerExceptionMessage);

        }
    }
}
