using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers
{
    public class WhenTriggeringProcessEmploymentChecksHttpTrigger
    {
        private Mock<HttpRequestMessage> _request;
        private Mock<IDurableOrchestrationClient> _starter;
        private Mock<ILogger> _logger;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _request = new Mock<HttpRequestMessage>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger>();
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_The_Instance_Id_Is_Created()
        {
            //Arrange
            var instanceId = _fixture.Create<string>();
            var expected = new HttpResponseMessage(HttpStatusCode.Accepted);

            _starter.Setup(x => x.StartNewAsync(nameof(ProcessEmploymentCheckRequestsOrchestrator), null))
                .ReturnsAsync(instanceId);
            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, instanceId, false))
                .Returns(expected);

            //Act
            var actual = await ProcessEmploymentChecksHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            //Assert
            
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task Then_The_Status_Code_Is_Returned()
        {
            //Arrange
            var expected = new HttpResponseMessage(HttpStatusCode.OK);

            _starter.Setup(x => x.StartNewAsync(nameof(EmploymentChecksOrchestrator), null))
                .ReturnsAsync("");
            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, It.IsAny<string>(), false))
                .Returns(expected);

            //Act
            var actual = await ProcessEmploymentChecksHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            //Assert
            Assert.AreEqual(expected.StatusCode, actual.StatusCode);
        }
    }
}