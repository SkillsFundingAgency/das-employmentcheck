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

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.CreateEmploymentCheckCacheRequestsOrchestratorTests
{
    public class WhenRunningCreateEmploymentCheckCacheRequestsOrchestrator
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>> _logger;
        private Models.EmploymentCheck _employmentCheck;
        private IList<Models.EmploymentCheck> _employmentChecks;
        private CreateEmploymentCheckCacheRequestsOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>>();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();
            _employmentChecks = new List<Models.EmploymentCheck> { _employmentCheck };
            _sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);
        }

        [Test]
        public async Task Then_The_SubOrchestrator_Is_Called()
        {
            // Arrange
            _context
                .Setup(a => a.CallActivityAsync<IList<Models.EmploymentCheck>>(nameof(GetEmploymentChecksBatchActivity), It.IsAny<object>()))
                .ReturnsAsync(_employmentChecks);

            _context
                .Setup(o => o.CallSubOrchestratorAsync<EmploymentCheckCacheRequest>(nameof(CreateEmploymentCheckCacheRequestOrchestrator), _employmentCheck));

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<IList<Models.EmploymentCheck>>(nameof(GetEmploymentChecksBatchActivity), It.IsAny<object>()), Times.Once);
            _context.Verify(o => o.CallSubOrchestratorAsync<EmploymentCheckCacheRequest>(nameof(CreateEmploymentCheckCacheRequestOrchestrator), _employmentCheck), Times.Once);
        }
    }
}