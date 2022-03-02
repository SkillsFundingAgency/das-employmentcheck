using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.EmploymentCheckHttpTriggerTests
{
    public class WhenTriggeringHttpEmploymentChecks
    {
        private Fixture _fixture;

        private Mock<ITriggerHelper> _triggerHelperMock;
        private Mock<HttpRequestMessage> _requestMock;
        private Mock<IDurableOrchestrationClient> _starterMock;
        private Mock<ILogger> _loggerMock;

        private string _createRequestsOrchestratorName;
        private string _createRequestsOrchestratorTriggerName;
        private string _createRequestsOrchestratorInstancePrefix;
        private string _processRequestsOrchestratorName;
        private string _processRequestsOrchestratorTriggerName;
        private string _processRequestsOrchestratorInstancePrefix;

        private string _orchestratorName;
        private string _triggerName;
        private string _instancePrefix;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _triggerHelperMock = new Mock<ITriggerHelper>();
            _requestMock = new Mock<HttpRequestMessage>();
            _starterMock = new Mock<IDurableOrchestrationClient>();
            _loggerMock = new Mock<ILogger>();

            _createRequestsOrchestratorName = _fixture.Create<string>();
            _createRequestsOrchestratorTriggerName = _fixture.Create<string>();
            _createRequestsOrchestratorInstancePrefix = _fixture.Create<string>();
            _processRequestsOrchestratorName = _fixture.Create<string>();
            _processRequestsOrchestratorTriggerName = _fixture.Create<string>();
            _processRequestsOrchestratorInstancePrefix = _fixture.Create<string>();

            _orchestratorName = _fixture.Create<string>();
            _triggerName = _fixture.Create<string>();
            _instancePrefix = _fixture.Create<string>();
        }

        [Test]
        public async Task Then_The_CreateRequests_And_ProcessRequests_Orchestrators_Are_Started_When_There_Is_No_Existing_Instance_Running()
        {
            // Arrange
            var instanceId = _fixture.Create<string>();
            var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent($"Started orchestrator [CreateEmploymentCheckCacheRequestsOrchestrator] with ID [{instanceId}]\n\n\n\n\nStarted orchestrator [ProcessEmploymentCheckRequestsOrchestrator] with ID [{instanceId}]\n\n\n\n") };
            var createRequestsOrchestratorResponse = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("") };
            var processRequestsOrchestratorResponse = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("") };
            var instances = new OrchestrationStatusQueryResult { DurableOrchestrationState = new List<DurableOrchestrationStatus>(0) };

            _triggerHelperMock
                .Setup(t => t.StartTheEmploymentCheckOrchestrators(
                    _requestMock.Object,
                    _starterMock.Object,
                    _loggerMock.Object,
                    _triggerHelperMock.Object))
                .ReturnsAsync(response);

            _triggerHelperMock
                .SetupSequence(t => t.StartOrchestrator(
                    _requestMock.Object,
                    _starterMock.Object,
                    _loggerMock.Object,
                    _triggerHelperMock.Object,
                    _orchestratorName,
                    _triggerName,
                    _instancePrefix))
             .ReturnsAsync(createRequestsOrchestratorResponse)
             .ReturnsAsync(processRequestsOrchestratorResponse);

            _triggerHelperMock
                .SetupSequence(t => t.GetRunningInstances(_triggerName, _instancePrefix, _starterMock.Object, _loggerMock.Object))
                .ReturnsAsync(() => null);

            _starterMock
                .Setup(s => s.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(instances);

            _starterMock
                 .Setup(s => s.StartNewAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(instanceId);

            _starterMock
                .SetupSequence(s => s.CreateCheckStatusResponse(_requestMock.Object, instanceId, false))
                .Returns(createRequestsOrchestratorResponse)
                .Returns(processRequestsOrchestratorResponse);

            // Act
            var result = await EmploymentChecksHttpTrigger.HttpStart(_requestMock.Object, _starterMock.Object, _loggerMock.Object);

            // Assert
            var resultContent = await result.Content.ReadAsStringAsync();
            var expectedContent = await response.Content.ReadAsStringAsync();
            Assert.AreEqual(expectedContent, resultContent);
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }

        [Test]
        public async Task And_When_The_CreateRequests_Orchestrator_Is_Already_Running_The_Trigger_Exits_Without_Starting_Another_Instance_Of_The_CreateRequests_Orchestrator()
        {
            // Arrange
            var instanceId = _fixture.Create<string>();
            var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent($"Started orchestrator [CreateEmploymentCheckCacheRequestsOrchestrator] with ID [{instanceId}]\n\n\n\n\nStarted orchestrator [ProcessEmploymentCheckRequestsOrchestrator] with ID [{instanceId}]\n\n\n\n") };
            var createRequestsOrchestratorResponse = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("") };
            var processRequestsOrchestratorResponse = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("") };
            var instances = new OrchestrationStatusQueryResult { DurableOrchestrationState = new[] { new DurableOrchestrationStatus() } };

            _triggerHelperMock
                .Setup(t => t.StartTheEmploymentCheckOrchestrators(
                    _requestMock.Object,
                    _starterMock.Object,
                    _loggerMock.Object,
                    _triggerHelperMock.Object))
                .ReturnsAsync(response);

            _triggerHelperMock
                .SetupSequence(t => t.StartOrchestrator(
                    _requestMock.Object,
                    _starterMock.Object,
                    _loggerMock.Object,
                    _triggerHelperMock.Object,
                    _orchestratorName,
                    _triggerName,
                    _instancePrefix))
             .ReturnsAsync(createRequestsOrchestratorResponse)
             .ReturnsAsync(processRequestsOrchestratorResponse);

            _triggerHelperMock
                 .SetupSequence(t => t.GetRunningInstances(_triggerName, _instancePrefix, _starterMock.Object, _loggerMock.Object))
                 .ReturnsAsync(() => null);

            _starterMock
                .Setup(s => s.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(instances);

            _starterMock
                 .Setup(s => s.StartNewAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(instanceId);

            _starterMock
                .SetupSequence(s => s.CreateCheckStatusResponse(_requestMock.Object, instanceId, false))
                .Returns(createRequestsOrchestratorResponse)
                .Returns(processRequestsOrchestratorResponse);

            // Act
            var result = await EmploymentChecksHttpTrigger.HttpStart(_requestMock.Object, _starterMock.Object, _loggerMock.Object);

            // Assert
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.AreEqual("Unable to start the CreateRequests Orchestrator: An instance of CreateEmploymentCheckCacheRequestsOrchestrator is already running.\n", resultContent);
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
        }

        [Test]
        public async Task And_When_The_ProcessRequest_Orchestrator_Is_Already_Running_The_Trigger_Exits_Without_Starting_Another_Instance_Of_The_ProcessRequests_Orchestrator()
        {
            // Arrange
            var instanceId = _fixture.Create<string>();
            var response = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent($"Started orchestrator [CreateEmploymentCheckCacheRequestsOrchestrator] with ID [{instanceId}]\n\n\n\n\nStarted orchestrator [ProcessEmploymentCheckRequestsOrchestrator] with ID [{instanceId}]\n\n\n\n") };
            var createRequestsOrchestratorResponse = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("") };
            var processRequestsOrchestratorResponse = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent("") };

            _triggerHelperMock
                .Setup(t => t.StartTheEmploymentCheckOrchestrators(
                    _requestMock.Object,
                    _starterMock.Object,
                    _loggerMock.Object,
                    _triggerHelperMock.Object))
                .ReturnsAsync(response);

            _triggerHelperMock
                .SetupSequence(t => t.StartOrchestrator(
                    _requestMock.Object,
                    _starterMock.Object,
                    _loggerMock.Object,
                    _triggerHelperMock.Object,
                    _orchestratorName,
                    _triggerName,
                    _instancePrefix))
             .ReturnsAsync(createRequestsOrchestratorResponse)
             .ReturnsAsync(processRequestsOrchestratorResponse);

            _triggerHelperMock
                 .SetupSequence(t => t.GetRunningInstances(_triggerName, _instancePrefix, _starterMock.Object, _loggerMock.Object))
                 .ReturnsAsync(() => null)
                 .ReturnsAsync(new OrchestrationStatusQueryResult { DurableOrchestrationState = new[] { new DurableOrchestrationStatus()}});

            _starterMock
                .SetupSequence(s => s.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new OrchestrationStatusQueryResult { DurableOrchestrationState = new List<DurableOrchestrationStatus>(0) })
                .ReturnsAsync(new OrchestrationStatusQueryResult { DurableOrchestrationState = new[] { new DurableOrchestrationStatus()}});

            _starterMock
                 .Setup(s => s.StartNewAsync(It.IsAny<string>(), It.IsAny<string>()))
                 .ReturnsAsync(instanceId);

            _starterMock
                .SetupSequence(s => s.CreateCheckStatusResponse(_requestMock.Object, instanceId, false))
                .Returns(createRequestsOrchestratorResponse)
                .Returns(processRequestsOrchestratorResponse);

            // Act
            var result = await EmploymentChecksHttpTrigger.HttpStart(_requestMock.Object, _starterMock.Object, _loggerMock.Object);

            // Assert
            var resultContent = await result.Content.ReadAsStringAsync();
            Assert.AreEqual("Unable to start the ProcessRequests Orchestrator: [An instance of ProcessEmploymentCheckRequestsOrchestrator is already running.]\n", resultContent);
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
        }
    }
}