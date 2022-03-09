using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.ProcessEmploymentCheckRequestsWithRateLimiterOrchestratorTests
{
    public class WhenRunningProcessEmploymentCheckRequestsWithRateLimiterOrchestrator
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<ILogger<ProcessEmploymentCheckRequestsOrchestrator>> _logger;

        private EmploymentCheckCacheRequest _employmentCheckCacheRequest;
        private IList<Data.Models.EmploymentCheck> _employmentChecks;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<ProcessEmploymentCheckRequestsOrchestrator>>();

            _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();
            _employmentChecks = new List<Data.Models.EmploymentCheck> { _fixture.Create<Data.Models.EmploymentCheck>() };
            _employmentChecks = new List<Data.Models.EmploymentCheck> { _fixture.Create<Data.Models.EmploymentCheck>() };
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            var sut = new ProcessEmploymentCheckRequestsOrchestrator(_logger.Object);

            _context
                .Setup(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null))
                .ReturnsAsync(_employmentCheckCacheRequest);

            _context
                .Setup(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), _employmentChecks))
                .ReturnsAsync(_employmentCheckCacheRequest);

            _context
                .Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), It.IsAny<EmploymentCheckData>()));

            // Act
            await sut.ProcessEmploymentCheckRequestsOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetEmploymentCheckCacheRequestActivity), null), Times.Once);
        }
    }
}