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
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.EmploymentCheckHttpTriggerTests
{
    public class WhenBuildingTheResult
    {
        private Mock<HttpRequestMessage> _request;
        private Mock<IDurableOrchestrationClient> _starter;
        private Mock<ILogger> _logger;
        private Mock<IEmploymentChecksHttpTriggerHelper> _employmentChecksHttpTriggerHelper;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _request = new Mock<HttpRequestMessage>(MockBehavior.Strict);
            _starter = new Mock<IDurableOrchestrationClient>(MockBehavior.Strict);
            _logger = new Mock<ILogger>();
            _employmentChecksHttpTriggerHelper = new Mock<IEmploymentChecksHttpTriggerHelper>(MockBehavior.Strict);
            _fixture = new Fixture();
        }

        [Test]
        public async Task Then_When_Building_Result_The_Response_Messages_From_Both_The_Create_And_Process_Request_Orchestrators_Are_Returned()
        {
            // Arrange
            string createInstanceId = _fixture.Create<string>();
            string processInstanceId = _fixture.Create<string>();
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
            var httpResponseMessage = new Tuple<string, HttpResponseMessage>("Test", response);
            var orchestratorName = _fixture.Create<string>();
            var inputMessage = "Started Orchestrator[CreateEmploymentCheckCacheRequestsOrchestrator] Id[CreateEmploymentCheck - e27caa76 - 578f - 4ec4 - b54f - 701b91c10561] : StatusCode: 202, ReasonPhrase: 'Accepted', 1234567890123456789012345678901234567890123456789012345678901234567890";
            var outputMessage = "Started Orchestrator[CreateEmploymentCheckCacheRequestsOrchestrator] Id[CreateEmploymentCheck - e27caa76 - 578f - 4ec4 - b54f - 701b91c10561] : StatusCode: 202, ReasonPhrase: 'Accepted'";

            var createResponse = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent(inputMessage) };
            var processResponse = new HttpResponseMessage(HttpStatusCode.Accepted) { Content = new StringContent(outputMessage) };

            Tuple<string, HttpResponseMessage> createTuple = new Tuple<string, HttpResponseMessage>(createInstanceId, createResponse);
            Tuple<string, HttpResponseMessage> processTuple = new Tuple<string, HttpResponseMessage>(processInstanceId, processResponse);

            _employmentChecksHttpTriggerHelper
                .Setup(x => x.FormatResponseString(httpResponseMessage, orchestratorName))
                .ReturnsAsync(outputMessage);

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

            string gg = result.ToString();
            _ = gg;

            // Assert
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }
    }
}