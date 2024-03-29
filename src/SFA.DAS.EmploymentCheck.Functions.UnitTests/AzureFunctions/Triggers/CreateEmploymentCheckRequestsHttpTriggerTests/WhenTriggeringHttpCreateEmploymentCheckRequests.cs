﻿using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
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
        private Mock<HttpRequestMessage> _request;
        private Mock<IDurableOrchestrationClient> _starter;
        private Mock<ILogger> _logger;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _request = new Mock<HttpRequestMessage>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger>();
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_The_Instance_Id_Is_Created_When_no_other_instances_are_running()
        {
            // Arrange
            string instanceId = _fixture.Create<string>();
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);

            _starter
                .Setup(x => x.StartNewAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(instanceId);

            _starter.Setup(x => x.CreateCheckStatusResponse(_request.Object, instanceId, false))
                .Returns(response);


            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new List<DurableOrchestrationStatus>(0)
            };

            _starter
                .Setup(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), CancellationToken.None))
                .ReturnsAsync(instances);
            

            // Act
            var result = await CreateEmploymentCheckRequestsOrchestratorHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            // Assert

            Assert.AreEqual(response, result);
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }

        [Test]
        public async Task Then_The_Instance_Id_Is_Not_Created_When_other_instances_are_running()
        {
            // Arrange
            string instanceId = _fixture.Create<string>();

            _starter
                .Setup(x => x.StartNewAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(instanceId);

            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new[] { new DurableOrchestrationStatus() }
            };

            _starter
                .Setup(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), CancellationToken.None))
                .ReturnsAsync(instances);

            // Act
            var result = await CreateEmploymentCheckRequestsOrchestratorHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            // Assert
            Assert.AreEqual(HttpStatusCode.Conflict, result.StatusCode);
        }
    }
}