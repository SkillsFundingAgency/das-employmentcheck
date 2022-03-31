using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.TriggerHelperTests
{
    public class WhenRunningStartOrchestrator
    {
        private Fixture _fixture;

        private Mock<ITriggerHelper> _triggerHelperMock;
        private Mock<HttpRequestMessage> _requestMock;
        private Mock<IDurableOrchestrationClient> _starterMock;
        private Mock<ILogger> _loggerMock;

        private string _orchestratorName;
        private string _triggerName;
        private string _instanceId;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _triggerHelperMock = new Mock<ITriggerHelper>();
            _requestMock = new Mock<HttpRequestMessage>();
            _starterMock = new Mock<IDurableOrchestrationClient>();
            _loggerMock = new Mock<ILogger>();

            _orchestratorName = _fixture.Create<string>();
            _triggerName = _fixture.Create<string>();
            _instanceId = _fixture.Create<string>();
        }

        [Test]
        public async Task Then_If_An_Orchestrator_Instance_Is_Returned_An_Accepted_HttpResponseMessage_Is_Returned()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("") };

            _triggerHelperMock
                .Setup(t => t.StartOrchestrator(
                    _requestMock.Object,
                    _starterMock.Object,
                    _loggerMock.Object,
                    _triggerHelperMock.Object,
                    _orchestratorName,
                    _triggerName))
             .ReturnsAsync(response);

            _starterMock
                 .Setup(s => s.StartNewAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(_instanceId);

            _starterMock
                .SetupSequence(s => s.CreateCheckStatusResponse(_requestMock.Object, _instanceId, false))
                .Returns(response);

            var sut = new TriggerHelper();

            // Act
            var result = await sut.StartOrchestrator(
                _requestMock.Object,
                _starterMock.Object,
                _loggerMock.Object,
                _triggerHelperMock.Object,
                _orchestratorName,
                _triggerName);

            // Assert
            var resultContent = await result.Content.ReadAsStringAsync();
            var expectedContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedContent, resultContent);
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }

        [Test]
        public async Task Then_If_An_Orchestrator_Instance_Is_Not_Returned_A_Conflict_HttpResponseMessage_Is_Returned()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Conflict) { Content = new StringContent($"An error occured starting [{_orchestratorName}], no instance id was returned.") };

            _triggerHelperMock
                .Setup(t => t.StartOrchestrator(
                    _requestMock.Object,
                    _starterMock.Object,
                    _loggerMock.Object,
                    _triggerHelperMock.Object,
                    _orchestratorName,
                    _triggerName))
             .ReturnsAsync(response);

            _starterMock
                 .Setup(s => s.StartNewAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(() => null);

            _starterMock
                .SetupSequence(s => s.CreateCheckStatusResponse(_requestMock.Object, _instanceId, false))
                .Returns(response);

            var sut = new TriggerHelper();

            // Act
            var result = await sut.StartOrchestrator(
                _requestMock.Object,
                _starterMock.Object,
                _loggerMock.Object,
                _triggerHelperMock.Object,
                _orchestratorName,
                _triggerName);

            // Assert
            var resultContent = await result.Content.ReadAsStringAsync();
            var expectedContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedContent, resultContent);
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }
    }
}