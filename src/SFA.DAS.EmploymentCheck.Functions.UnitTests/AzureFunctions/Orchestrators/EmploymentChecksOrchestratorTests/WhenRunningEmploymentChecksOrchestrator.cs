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
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<ILogger<EmploymentChecksOrchestrator>> _logger;

        [SetUp]
        public void SetUp()
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
            _context.Setup(a => a.CallSubOrchestratorAsync(nameof(ProcessEmploymentCheckRequestsOrchestrator), It.IsAny<object>()));

            // Act
            await sut.EmploymentChecksOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallSubOrchestratorAsync(nameof(CreateEmploymentCheckCacheRequestsOrchestrator), It.IsAny<object>()), Times.Once);
            _context.Verify(a => a.CallSubOrchestratorAsync(nameof(ProcessEmploymentCheckRequestsOrchestrator), It.IsAny<object>()), Times.Once);
        }
    }
}