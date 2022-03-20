using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.CreateEmploymentCheckCacheRequestsOrchestratorTests
{
    public class WhenResponseOrchestratorIsTriggered
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<ILogger<ResponseOrchestrator>> _logger;

        private Data.Models.EmploymentCheck _employmentCheck;

        private ResponseOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<ResponseOrchestrator>>();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();

            _sut = new ResponseOrchestrator(_logger.Object);
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            _context
                .SetupSequence(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetResponseEmploymentCheckActivity), null))
                .ReturnsAsync(_employmentCheck)
                .ReturnsAsync( () => null);

            // Act
            await _sut.ResponseOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetResponseEmploymentCheckActivity), null), Times.Exactly(2));
        }
    }
}