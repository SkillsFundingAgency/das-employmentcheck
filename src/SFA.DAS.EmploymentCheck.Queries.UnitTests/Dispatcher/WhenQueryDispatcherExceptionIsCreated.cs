using FluentAssertions;
using NUnit.Framework;
using System.Text.Json;
using Newtonsoft.Json;

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
        public void Then_Create_QueryDispatcherException_Using_Serialisation()
        {
            // Arrange
            var expectedMessage = "ExceptionTest Message";
            var queryDispatcherException = new QueryDispatcherException(expectedMessage);

            var json = JsonConvert.SerializeObject(queryDispatcherException);
            var actual = JsonConvert.DeserializeObject<QueryDispatcherException>(json);

            // Assert
            actual.Should().NotBeNull();
            actual.Message.Should().Be(expectedMessage);
        }
    }
}