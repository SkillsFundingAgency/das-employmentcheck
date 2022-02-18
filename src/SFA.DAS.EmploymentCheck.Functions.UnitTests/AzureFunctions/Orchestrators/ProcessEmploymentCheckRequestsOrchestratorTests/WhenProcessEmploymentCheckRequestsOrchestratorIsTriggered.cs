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
            // Act
            await _sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetEmploymentCheckCacheRequestActivity", null), Times.Once);
        }

        [Test]
        public async Task Then_GetHmrcLearnerEmploymentStatusActivity_Is_Not_Called_When_Nothing_To_Process()
        {
            // Arrange
            _mockOrchestrationContext.Setup(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest>("GetEmploymentCheckCacheRequestActivity", null))
                .ReturnsAsync((EmploymentCheckCacheRequest)null);
            
            // Act
            await _sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetHmrcLearnerEmploymentStatusActivity", It.IsAny<EmploymentCheckCacheRequest>()), Times.Never);
        }

        [Test]
        public async Task Then_GetHmrcLearnerEmploymentStatusActivity_Is_Called()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest>();
            _mockOrchestrationContext.Setup(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest>("GetEmploymentCheckCacheRequestActivity", null)
                )
                .ReturnsAsync(request);

            // Act
            await _sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetHmrcLearnerEmploymentStatusActivity", request), Times.Once);
        }

        [Test]
        public async Task Then_Any_Exceptions_Are_Caught_And_Logged()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _mockOrchestrationContext.Setup(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest>("GetEmploymentCheckCacheRequestActivity", null)
                )
                .Throws(exception);

            // Act
            await _sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockLogger.VerifyLog(LogLevel.Error, Times.Once(), $"\n\n{nameof(ProcessEmploymentCheckRequestsOrchestrator)} Exception caught: {exception.Message}. {exception.StackTrace}");
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetHmrcLearnerEmploymentStatusActivity", It.IsAny<EmploymentCheckCacheRequest>()), Times.Never);
        }
    }
}
