using System;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators
{
    public class WhenProcessEmploymentCheckRequestsOrchestratorIsTriggered
    {
        private ProcessEmploymentCheckRequestsOrchestrator _sut;
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private Mock<ILogger<ProcessEmploymentCheckRequestsOrchestrator>> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockLogger = new Mock<ILogger<ProcessEmploymentCheckRequestsOrchestrator>>();

            _sut = new ProcessEmploymentCheckRequestsOrchestrator(_mockLogger.Object);
        }

        [Test]
        public async Task Then_GetEmploymentCheckCacheRequestActivity_Is_Called()
        {
            await _sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetEmploymentCheckCacheRequestActivity", null), Times.Once);
        }

        [Test]
        public async Task Then_GetHmrcLearnerEmploymentStatusActivity_Is_Not_Called_When_Nothing_To_Process()
        {
            _mockOrchestrationContext.Setup(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest>("GetEmploymentCheckCacheRequestActivity", null))
                .ReturnsAsync((EmploymentCheckCacheRequest)null);

            await _sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetHmrcLearnerEmploymentStatusActivity", It.IsAny<EmploymentCheckCacheRequest>()), Times.Never);
        }

        [Test]
        public async Task Then_GetHmrcLearnerEmploymentStatusActivity_Is_Called()
        {
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            _mockOrchestrationContext.Setup(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest>("GetEmploymentCheckCacheRequestActivity", null)
                )
                .ReturnsAsync(request);

            await _sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_mockOrchestrationContext.Object);

            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetHmrcLearnerEmploymentStatusActivity", request), Times.Once);
        }

        [Test]
        public async Task Then_Any_Exceptions_Are_Caught_And_Logged()
        {
            var exception = _fixture.Create<Exception>();
            _mockOrchestrationContext.Setup(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest>("GetEmploymentCheckCacheRequestActivity", null)
                )
                .Throws(exception);

            await _sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_mockOrchestrationContext.Object);

            _mockLogger.VerifyLog(LogLevel.Error, Times.Once(), $"\n\n{nameof(ProcessEmploymentCheckRequestsOrchestrator)} Exception caught: {exception.Message}. {exception.StackTrace}");
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetHmrcLearnerEmploymentStatusActivity", It.IsAny<EmploymentCheckCacheRequest>()), Times.Never);
        }
    }
}
