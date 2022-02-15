using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.ProcessEmploymentCheckRequestsWithRateLimiterOrchestratorTests
{
    public class WhenRunningProcessEmploymentCheckRequestsWithRateLimiterOrchestrator
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<ILogger<ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator>> _logger;

        private EmploymentCheckCacheRequest employmentCheckCacheRequest;
        private IList<Models.EmploymentCheck> _employmentChecks;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator>>();

            employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            _employmentChecks = new List<Models.EmploymentCheck> { _fixture.Create<Models.EmploymentCheck>() };
            _employmentChecks = new List<Models.EmploymentCheck> { _fixture.Create<Models.EmploymentCheck>() };
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            var sut = new ProcessEmploymentCheckRequestsWithRateLimiterOrchestrator(_logger.Object);

            _context
                .Setup(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null))
                .ReturnsAsync(employmentCheckCacheRequest);

            _context
                .Setup(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), _employmentChecks))
                .ReturnsAsync(employmentCheckCacheRequest);

            _context
                .Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestsActivity), It.IsAny<EmploymentCheckData>()));

            // Act
            await sut.ProcessEmploymentChecksWithRateLimiterOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null), Times.Once);
        }
    }
}