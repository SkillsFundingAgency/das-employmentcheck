using AutoFixture;
using FluentAssertions;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.EmploymentCheckHttpTriggerTests
{
    public class WhenRunningStartTheCreateAndProcessRequestOrchestrators
    {
        private Mock<HttpRequestMessage> _request;
        private Mock<IDurableOrchestrationClient> _starter;
        private Mock<ILogger> _logger;
        private Mock<IEmploymentChecksHttpTriggerHelper> _employmentChecksHttpTriggerHelper;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _request = new Mock<HttpRequestMessage>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _logger = new Mock<ILogger>();
            _employmentChecksHttpTriggerHelper = new Mock<IEmploymentChecksHttpTriggerHelper>();
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_The_Response_Messages_From_Both_The_Create_And_Process_Request_Orchestrators_Are_Returned()
        {
            // Arrange
            string createInstanceId = _fixture.Create<string>();
            string processInstanceId = _fixture.Create<string>();
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);

            var createResponse = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent($"Started Orchestrator[{nameof(CreateEmploymentCheckCacheRequestsOrchestrator)}] Id[1234] : ")
            };

            var processResponse = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent($"Started Orchestrator[{nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator)}] Id[5678] : ")
            };

            Tuple<string, HttpResponseMessage> createTuple = new Tuple<string, HttpResponseMessage>(createInstanceId, createResponse);
            Tuple<string, HttpResponseMessage> processTuple = new Tuple<string, HttpResponseMessage>(processInstanceId, processResponse);

            //_employmentChecksHttpTriggerHelper
            //    .Setup(x => x.FormatResponseString)

            _employmentChecksHttpTriggerHelper
                .Setup(x => x.StartCreateRequestsOrchestrator(_request.Object, _starter.Object, _logger.Object))
                .ReturnsAsync(createTuple);

            _employmentChecksHttpTriggerHelper
                .Setup(x => x.StartProcessRequestsOrchestrator(_request.Object, _starter.Object, _logger.Object))
                .ReturnsAsync(processTuple);

            _starter
                .Setup(x => x.StartNewAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(createInstanceId);

            _starter
                .Setup(x => x.StartNewAsync(nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(processInstanceId);

            _starter
                .Setup(x => x.CreateCheckStatusResponse(_request.Object, createInstanceId, false))
                .Returns(response);

            _starter
                .Setup(x => x.CreateCheckStatusResponse(_request.Object, processInstanceId, false))
                .Returns(response);

            var instances = new OrchestrationStatusQueryResult
            {
                DurableOrchestrationState = new List<DurableOrchestrationStatus>(0)
            };

            _starter
                .Setup(x => x.ListInstancesAsync(It.IsAny<OrchestrationStatusQueryCondition>(), CancellationToken.None))
                .ReturnsAsync(instances);

            // Act
            var result = await EmploymentChecksHttpTrigger.HttpStart(_request.Object, _starter.Object, _logger.Object);

            // Assert
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }
    }
}