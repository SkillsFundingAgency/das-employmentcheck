using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Triggers.EmploymentCheckHttpTriggerTests
{
    public class WhenTriggeringHttpEmploymentCheck
    {
        private readonly Mock<HttpRequestMessage> _request;
        private readonly Mock<IDurableOrchestrationClient> _starter;
        private readonly Mock<ILogger> _logger;

        public WhenTriggeringHttpEmploymentCheck()
        {
            _request = new Mock<HttpRequestMessage>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger>();
        }

        [Fact]
        public async void Then_The_Instance_Id_Is_Created()
        {
            //Arrange
            var instanceId = "test";
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);

            _starter.Setup(x => x.StartNewAsync(nameof(TestEmploymentCheckOrchestrator), null))
                .ReturnsAsync(instanceId);
            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, instanceId, false))
                .Returns(response);

            //Act
            var result = await EmploymentCheckHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            //Assert
            
            Assert.Equal(response, result);
        }

        [Fact]
        public async void Then_The_Status_Code_Is_Returned()
        {
            //Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            _starter.Setup(x => x.StartNewAsync(nameof(TestEmploymentCheckOrchestrator), null))
                .ReturnsAsync("");
            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, It.IsAny<string>(), false))
                .Returns(response);

            //Act
            var result = await EmploymentCheckHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            //Assert
            Assert.Equal(response.StatusCode, result.StatusCode);
        }
    }
}