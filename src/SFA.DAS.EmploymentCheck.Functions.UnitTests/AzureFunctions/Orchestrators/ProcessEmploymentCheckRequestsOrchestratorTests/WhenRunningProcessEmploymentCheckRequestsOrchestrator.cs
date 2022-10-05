using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.ProcessEmploymentCheckRequestsOrchestratorTests
{
    public class WhenRunningProcessEmploymentCheckRequestsOrchestrator
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<ILogger<ProcessEmploymentCheckRequestsOrchestrator>> _logger;

        private EmploymentCheckCacheRequest[] _employmentCheckCacheRequest;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<ProcessEmploymentCheckRequestsOrchestrator>>();
            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest[]>();
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            var sut = new ProcessEmploymentCheckRequestsOrchestrator(_logger.Object);

            _context
                .SetupSequence(a => a.CallActivityAsync<EmploymentCheckCacheRequest[]>(nameof(GetEmploymentCheckCacheRequestActivity), null))
                .ReturnsAsync(_employmentCheckCacheRequest)
                .ReturnsAsync(() => null);

            // Act
            await sut.ProcessEmploymentCheckRequestsOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<EmploymentCheckCacheRequest[]>(nameof(GetEmploymentCheckCacheRequestActivity), null), Times.Exactly(2));
            _context.Verify(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), It.IsAny<EmploymentCheckCacheRequest>()), Times.Exactly(_employmentCheckCacheRequest.Length));
            _context.Verify(a => a.CallActivityAsync(nameof(AbandonRelatedRequestsActivity), It.IsAny<EmploymentCheckCacheRequest[]>()), Times.Once);
        }
    }
}