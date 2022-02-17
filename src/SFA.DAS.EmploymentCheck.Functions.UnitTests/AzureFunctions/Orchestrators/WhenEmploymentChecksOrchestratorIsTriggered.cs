using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators
{
    public class WhenEmploymentChecksOrchestratorIsTriggered
    {
        private EmploymentChecksOrchestrator _sut;
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _mockOrchestrationContext;
        private Mock<ILogger<EmploymentChecksOrchestrator>> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mockOrchestrationContext = new Mock<IDurableOrchestrationContext>();
            _mockLogger = new Mock<ILogger<EmploymentChecksOrchestrator>>();

            _sut = new EmploymentChecksOrchestrator(_mockLogger.Object);
        }

        [Test]
        public async Task Then_CreateEmploymentCheckCacheRequestsOrchestrator_Is_Called()
        {
            // Act
            await _sut.EmploymentChecksOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallSubOrchestratorAsync("CreateEmploymentCheckCacheRequestsOrchestrator", 0), Times.Once);
        }

        [Test]
        public async Task Then_ProcessEmploymentCheckRequestsOrchestrator_Is_Called()
        {
            // Act
            await _sut.EmploymentChecksOrchestratorTask(_mockOrchestrationContext.Object);

            // Assert
            _mockOrchestrationContext.Verify(c => c.CallSubOrchestratorAsync("ProcessEmploymentCheckRequestsOrchestrator", 0), Times.Once);
        }

        [Test]
        public async Task Then_Any_Exceptions_Are_Caught_And_Logged()
        {
            var exception = _fixture.Create<Exception>();
            _mockOrchestrationContext.Setup(c =>
                    c.CallSubOrchestratorAsync("CreateEmploymentCheckCacheRequestsOrchestrator", 0)
                )
                .Throws(exception);

            await _sut.EmploymentChecksOrchestratorTask(_mockOrchestrationContext.Object);

            _mockLogger.VerifyLog(LogLevel.Error, Times.Once(), $"\n\nEmploymentChecksOrchestrator.EmploymentChecksOrchestratorTask: Exception caught - {exception.Message}. {exception.StackTrace}");
        }
    }
}
