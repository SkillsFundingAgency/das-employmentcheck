using Microsoft.ApplicationInsights.Channel;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Telemetry
{
    public class WhenInitializingTelemetry
    {
        [Test]
        public void Then_The_HMRC_API_Telemetry_Sanitizer_Is_Called()
        {
            //Arrange
            var sanitizer = new Mock<IHmrcApiTelemetrySanitizer>();
            var sut = new TelemetryIntializer(sanitizer.Object);

            //Act
            sut.Initialize(It.IsAny<ITelemetry>());

            //Assert
            sanitizer.Verify(x => x.ProcessHmrcApiTelemetry(It.IsAny<ITelemetry>()), Times.Once);
        }
    }
}