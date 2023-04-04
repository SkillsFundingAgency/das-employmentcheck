using Microsoft.ApplicationInsights.Channel;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Telemetry
{
    public class WhenInitializingTelemetry
    {
        private Mock<IHmrcApiTelemetrySanitizer> _hmrcApiTelemetrySanitizerMock;
        private Mock<ILearnerDataTelemetrySanitizer> _learnerDataTelemetrySanitizerMock;

        [SetUp]
        public void SetUp()
        {
            _hmrcApiTelemetrySanitizerMock = new Mock<IHmrcApiTelemetrySanitizer>();
            _learnerDataTelemetrySanitizerMock = new Mock<ILearnerDataTelemetrySanitizer>();
        }

        [Test]
        public void Then_The_HMRC_API_Telemetry_Sanitizer_Is_Called()
        {
            //Arrange
            var sut = new TelemetryIntializer(_hmrcApiTelemetrySanitizerMock.Object, _learnerDataTelemetrySanitizerMock.Object);
            
            //Act
            sut.Initialize(It.IsAny<ITelemetry>());

            //Assert
            _hmrcApiTelemetrySanitizerMock.Verify(x => x.ProcessHmrcApiTelemetry(It.IsAny<ITelemetry>()), Times.Once);
        }

        [Test]
        public void Then_The_Learner_Data_API_Telemetry_Sanitizer_Is_Called()
        {
            //Arrange
            var sut = new TelemetryIntializer(_hmrcApiTelemetrySanitizerMock.Object, _learnerDataTelemetrySanitizerMock.Object);

            //Act
            sut.Initialize(It.IsAny<ITelemetry>());

            //Assert
            _learnerDataTelemetrySanitizerMock.Verify(x => x.ProcessLearnerDataTelemetry(It.IsAny<ITelemetry>()), Times.Once);
        }
    }
}