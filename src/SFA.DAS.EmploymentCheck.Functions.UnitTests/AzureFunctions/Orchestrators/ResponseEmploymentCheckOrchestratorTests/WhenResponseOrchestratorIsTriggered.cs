using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.ResponseEmploymentCheckOrchestratorTests
{
    public class WhenResponseOrchestratorIsTriggered
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private ResponseOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();

            _sut = new ResponseOrchestrator();
        }

        [Test]
        public async Task Then_The_GetResponseEmploymentCheckActivity_Is_Called_Until_No_More_Completed_Checks_Left()
        {
            // Arrange
            _context
                .SetupSequence(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetResponseEmploymentCheckActivity), null))
                .ReturnsAsync(_fixture.Create<Data.Models.EmploymentCheck>())
                .ReturnsAsync(_fixture.Create<Data.Models.EmploymentCheck>())
                .ReturnsAsync(() => null);

            // Act
            await _sut.ResponseOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetResponseEmploymentCheckActivity), null), Times.Exactly(3));
        }

        [Test]
        public async Task Then_The_OutputEmploymentCheckResultsActivity_Is_Called()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            _context
                .SetupSequence(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetResponseEmploymentCheckActivity), null))
                .ReturnsAsync(employmentCheck)
                .ReturnsAsync(() => null);

            // Act
            await _sut.ResponseOrchestratorTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync(nameof(OutputEmploymentCheckResultsActivity), employmentCheck), Times.Exactly(1));
        }
    }
}