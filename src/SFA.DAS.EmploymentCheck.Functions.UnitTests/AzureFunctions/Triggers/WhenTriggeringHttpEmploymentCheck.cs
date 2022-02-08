using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers
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

        [Test]
        public async Task Then_The_Instance_Id_Is_Created()
        {
            //Arrange
            const string instanceId = "test";
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);

            _starter.Setup(x => x.StartNewAsync(nameof(EmploymentChecksOrchestrator), null))
                .ReturnsAsync(instanceId);
            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, instanceId, false))
                .Returns(response);

            //Act
            var result = await EmploymentChecksHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            //Assert
            
            Assert.AreEqual(response, result);
        }

        [Test]
        public async Task Then_The_Status_Code_Is_Returned()
        {
            //Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            _starter.Setup(x => x.StartNewAsync(nameof(EmploymentChecksOrchestrator), null))
                .ReturnsAsync("");
            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, It.IsAny<string>(), false))
                .Returns(response);

            //Act
            var result = await EmploymentChecksHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            //Assert
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }
    }
}