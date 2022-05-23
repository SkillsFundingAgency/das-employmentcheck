using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Triggers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Triggers.ResetEmploymentChecksMessageSentDateHttpTriggerTests
{
    public class WhenTriggeringHttpResetEmploymentChecksMessageSentDate
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<IDurableOrchestrationClient> _starter;
        private Mock<HttpRequestMessage> _request;
        private Mock<ILogger> _logger;
        private ResetEmploymentChecksMessageSentDateOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _starter = new Mock<IDurableOrchestrationClient>();
            _request = new Mock<HttpRequestMessage>();
            _logger = new Mock<ILogger>();

            _sut = new ResetEmploymentChecksMessageSentDateOrchestrator();
        }

        [Test]
        public async Task Then_The_Instance_Id_Is_Created()
        {
            // Arrange
            string instanceId = _fixture.Create<string>();
            var uri = new Uri("http://localhost:7071/api/orchestrators/ResetEmploymentChecksMessageSentDateOrchestratorHttpTrigger?CorrelationId=EE9DF079-0F35-45F1-8451-B05433A738C5");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var timeout = TimeSpan.FromSeconds(60);
            var retryInterval = TimeSpan.FromSeconds(5);

            _starter
                .Setup(x => x.StartNewAsync(nameof(ResetEmploymentChecksMessageSentDateOrchestrator), It.IsAny<string>()))
                .ReturnsAsync(instanceId);

            _starter
                .Setup(x => x.WaitForCompletionOrCreateCheckStatusResponseAsync(request, It.IsAny<string>(), timeout, retryInterval))
                .ReturnsAsync(response);

            // Act
            var result = await ResetEmploymentChecksMessageSentDateOrchestratorHttpTrigger.HttpStart(request, _starter.Object, _logger.Object);

            // Assert
            Assert.AreEqual(response.StatusCode, result.StatusCode);
        }
    }
}