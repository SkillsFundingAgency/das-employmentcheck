using System;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.ProcessEmploymentCheckRequestsOrchestratorTests
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
            await _sut.ProcessEmploymentCheckRequestsOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest[]>("GetEmploymentCheckCacheRequestActivity", null), Times.Once);
        }

        [Test]
        public async Task Then_GetHmrcLearnerEmploymentStatusActivity_Is_Not_Called_When_Nothing_To_Process()
        {
            // Arrange
            _mockOrchestrationContext.SetupSequence(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest[]>("GetEmploymentCheckCacheRequestActivity", null))
                .ReturnsAsync(Array.Empty<EmploymentCheckCacheRequest>);
            
            // Act
            await _sut.ProcessEmploymentCheckRequestsOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest[]>("GetHmrcLearnerEmploymentStatusActivity", It.IsAny<EmploymentCheckCacheRequest>()), Times.Never);
        }

        [Test]
        public async Task Then_GetHmrcLearnerEmploymentStatusActivity_Is_Called()
        {
            // Arrange
            var request = _fixture.Create<EmploymentCheckCacheRequest[]>();
            _mockOrchestrationContext.SetupSequence(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest[]>("GetEmploymentCheckCacheRequestActivity", null)
                )
                .ReturnsAsync(request)
                .ReturnsAsync(Array.Empty<EmploymentCheckCacheRequest>);

            // Act
            await _sut.ProcessEmploymentCheckRequestsOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync("GetHmrcLearnerEmploymentStatusActivity", It.IsAny<EmploymentCheckCacheRequest>()), Times.Exactly(request.Length));
        }


        [Test]
        public async Task Then_message_indicating_nothing_to_process_is_logged()
        {
            // Arrange
            _mockOrchestrationContext.Setup(c =>
                    c.CallActivityAsync<EmploymentCheckCacheRequest[]>("GetEmploymentCheckCacheRequestActivity", null)
                )
                .ReturnsAsync(Array.Empty<EmploymentCheckCacheRequest>);

            // Act
            await _sut.ProcessEmploymentCheckRequestsOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallActivityAsync<EmploymentCheckCacheRequest>("GetHmrcLearnerEmploymentStatusActivity", It.IsAny<object>()), Times.Never);

            _mockLogger.VerifyLogContains(LogLevel.Information, Times.Once(), "ProcessEmploymentCheckRequestsOrchestrator: GetEmploymentCheckCacheRequestActivity returned no results. Nothing to process.");
        }
    }
}
