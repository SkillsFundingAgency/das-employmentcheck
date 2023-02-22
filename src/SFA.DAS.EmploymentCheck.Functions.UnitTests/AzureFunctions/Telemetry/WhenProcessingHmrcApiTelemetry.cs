using Microsoft.ApplicationInsights.DataContracts;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Telemetry;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Telemetry
{
    public class WhenProcessingHmrcApiTelemetry
    {
        [Test]
        public void Then_Dependencies_Which_Contain_Paye_Marker_Have_Data_And_Name_Sanitized()
        {
            //Arrange
            var dependencyTelemetry = new DependencyTelemetry
            {
                Data = "before/epaye/someextradata/extrapath?then=query",
                Name = "http://stub.hmrc.gov.uk/before/epaye/someextradata/extrapath?then=query"
            };

            var sut = new HmrcApiTelemetrySanitizer();

            //Act
            sut.ProcessHmrcApiTelemetry(dependencyTelemetry);

            //Assert
            Assert.AreEqual("before/epaye/", dependencyTelemetry.Data);
            Assert.AreEqual("http://stub.hmrc.gov.uk/before/epaye/", dependencyTelemetry.Name);
        }

        [Test]
        public void Then_Dependencies_Which_Do_Not_Contain_Paye_Marker_AreIgnored()
        {
            //Arrange
            var dependencyTelemetry = new DependencyTelemetry
            {
                Data = "/notpaye/someextradata",
                Name = "/notpaye/someextradatatoo"
            };

            var sut = new HmrcApiTelemetrySanitizer();

            //Act
            sut.ProcessHmrcApiTelemetry(dependencyTelemetry);

            //Assert
            Assert.AreEqual("/notpaye/someextradata", dependencyTelemetry.Data);
            Assert.AreEqual("/notpaye/someextradatatoo", dependencyTelemetry.Name);
        }


        [Test]
        public void Then_Requests_Are_Ignored()
        {
            //Arrange
            var requestTelemetry = new RequestTelemetry
            {
                Name = "/paye/SomeName",
                ResponseCode = "400",
                Success = false
            };

            var sut = new HmrcApiTelemetrySanitizer();

            //Act
            sut.ProcessHmrcApiTelemetry(requestTelemetry);

            //Assert
            Assert.AreEqual("/paye/SomeName", requestTelemetry.Name);
            Assert.AreEqual("400", requestTelemetry.ResponseCode);
            Assert.AreEqual(false, requestTelemetry.Success);
        }

        [Test]
        public void Then_Traces_Are_Ignored()
        {
            //Arrange
            var requestTelemetry = new TraceTelemetry
            {
                Message = "/paye/somemessage"
            };

            var sut = new HmrcApiTelemetrySanitizer();

            //Act
            sut.ProcessHmrcApiTelemetry(requestTelemetry);

            //Assert
            Assert.AreEqual("/paye/somemessage", requestTelemetry.Message);
        }
    }
}