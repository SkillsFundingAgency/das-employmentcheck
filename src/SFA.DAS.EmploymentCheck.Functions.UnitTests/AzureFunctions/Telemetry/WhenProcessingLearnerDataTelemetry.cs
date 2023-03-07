using Microsoft.ApplicationInsights.DataContracts;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Telemetry
{
    public class WhenProcessingLearnerDataTelemetry
    {
        [Test]
        public void Then_Dependencies_Which_Contain_ULN_Query_Have_Their_Data_Sanitized()
        {
            //Arrange
            var dependencyTelemetry = new DependencyTelemetry
            {
                Data = "https://somepath.co.uk/learnersNi/2122?ulns=1122334455",
                Name = "/learnersNI/2122?ulns=1122334455"
            };

            var sut = new LearnerDataTelemetrySanitizer();

            //Act
            sut.ProcessLearnerDataTelemetry(dependencyTelemetry);

            //Assert
            Assert.AreEqual("https://somepath.co.uk/learnersNi/2122", dependencyTelemetry.Data);
            Assert.AreEqual("/learnersNI/2122", dependencyTelemetry.Name);
        }

        [Test]
        public void Then_Dependencies_Which_Do_Not_Contain_Marker_AreIgnored()
        {
            //Arrange
            var dependencyTelemetry = new DependencyTelemetry
            {
                Data = "https://somepath.co.uk/learnersNi/2122?notulns=1122334455",
                Name = "/learnersNI/2122?notulns=1122334455"
            };

            var sut = new LearnerDataTelemetrySanitizer();

            //Act
            sut.ProcessLearnerDataTelemetry(dependencyTelemetry);

            //Assert
            Assert.AreEqual("https://somepath.co.uk/learnersNi/2122?notulns=1122334455", dependencyTelemetry.Data);
            Assert.AreEqual("/learnersNI/2122?notulns=1122334455", dependencyTelemetry.Name);
        }


        [Test]
        public void Then_Requests_Are_Ignored()
        {
            //Arrange
            var requestTelemetry = new RequestTelemetry
            {
                Name = "/learnersNI/2122?notulns=1122334455",
                ResponseCode = "400",
                Success = false
            };

            var sut = new LearnerDataTelemetrySanitizer();

            //Act
            sut.ProcessLearnerDataTelemetry(requestTelemetry);

            //Assert
            Assert.AreEqual("/learnersNI/2122?notulns=1122334455", requestTelemetry.Name);
            Assert.AreEqual("400", requestTelemetry.ResponseCode);
            Assert.AreEqual(false, requestTelemetry.Success);
        }
    }
}
