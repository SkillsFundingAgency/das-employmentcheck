using FluentAssertions;
using NUnit.Framework;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.Dispatcher
{
    public class WhenQueryDispatcherExceptionIsCreated
    {
        [Test]
        public void Then_Create_QueryDispatcherException_No_Parameters()
        {
            // Arrange and Act
            var queryDispatcherException = new QueryDispatcherException();

            // Assert
            queryDispatcherException.Should().NotBeNull();

        }

        [Test]
        public void Then_Create_QueryDispatcherException_Using_Serailisation()
        {
            // Arrange
            var expectedMessage = "ExceptionTest Message";
            var queryDispatcherException = new QueryDispatcherException(expectedMessage);
            QueryDispatcherException actual;

            // Act
            using (MemoryStream mem = new MemoryStream())
            {
                BinaryFormatter b = new BinaryFormatter();
                b.Serialize(mem, queryDispatcherException);
                mem.Seek(0, SeekOrigin.Begin);
                actual = b.Deserialize(mem) as QueryDispatcherException;
            }

            // Assert
            actual.Should().NotBeNull();
            actual.Message.Should().Be(expectedMessage);

        }
    }
}
