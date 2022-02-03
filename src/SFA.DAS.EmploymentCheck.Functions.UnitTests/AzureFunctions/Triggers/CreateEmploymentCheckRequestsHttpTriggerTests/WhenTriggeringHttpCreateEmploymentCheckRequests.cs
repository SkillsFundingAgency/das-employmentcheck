using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.CreateEmploymentCheckRequestsHttpTriggerTests
{
    public class WhenTriggeringHttpCreateEmploymentCheckRequests
    {
        private readonly Mock<HttpRequestMessage> _request;
        private readonly Mock<IDurableOrchestrationClient> _starter;
        private readonly Mock<ILogger> _logger;
        private readonly Mock<ITriggerHelper> _triggerHelper;
        private readonly Fixture _fixture;

        public WhenTriggeringHttpCreateEmploymentCheckRequests()
        {
            _request = new Mock<HttpRequestMessage>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger>();
            _triggerHelper = new Mock<ITriggerHelper>();
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_The_Instance_Id_Is_Created_When_no_other_instances_are_running()
        {
            //Arrange
            string instanceId = _fixture.Create<string>();
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);

            _starter.Setup(x => x.StartNewAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(instanceId);
            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, instanceId, false))
                .Returns(response);

            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new List<DurableOrchestrationStatus>(0)
            };

            _triggerHelper.Setup(x => x.GetRunningInstances(nameof(CreateEmploymentCheckRequestsOrchestratorTrigger),
                "CreateEmploymentCheck-", It.IsAny<IDurableOrchestrationClient>(), It.IsAny<ILogger>())).ReturnsAsync(instances);
            
            //Act
            var result = await CreateEmploymentCheckRequestsOrchestratorTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object, _triggerHelper.Object);

            //Assert

            Assert.AreEqual(response, result);
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }

        [Test]
        public async Task Then_The_Instance_Id_Is_Not_Created_When_other_instances_are_running()
        {
            //Arrange
            string instanceId = _fixture.Create<string>();

            _starter.Setup(x => x.StartNewAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(instanceId);

            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new[] { new DurableOrchestrationStatus() }
            };

            _triggerHelper.Setup(x => x.GetRunningInstances(nameof(CreateEmploymentCheckRequestsOrchestratorTrigger),
                "CreateEmploymentCheck-", It.IsAny<IDurableOrchestrationClient>(), It.IsAny<ILogger>())).ReturnsAsync(instances);

            //Act
            var result = await CreateEmploymentCheckRequestsOrchestratorTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object, _triggerHelper.Object);

            //Assert
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
        }
    }
}