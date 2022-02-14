using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.EmploymentChecksOrchestratorTests
{
    public class WhenRunningEmploymentChecksOrchestrator
    {
        private readonly Fixture _fixture;
        private readonly Mock<IDurableOrchestrationContext> _context;
        private readonly Mock<ILogger<EmploymentChecksOrchestrator>> _logger;

        public WhenRunningEmploymentChecksOrchestrator()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<EmploymentChecksOrchestrator>>();
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            var sut = new EmploymentChecksOrchestrator(_logger.Object);

            _context.Setup(a => a.CallSubOrchestratorAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), It.IsAny<object>()));
            _context.Setup(a => a.CallSubOrchestratorAsync(nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator), It.IsAny<object>()));

            // Act
            await sut.EmploymentChecksOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallSubOrchestratorAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), It.IsAny<object>()), Times.Once);
            _context.Verify(a => a.CallSubOrchestratorAsync(nameof(ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator), It.IsAny<object>()), Times.Once);
        }
    }
}