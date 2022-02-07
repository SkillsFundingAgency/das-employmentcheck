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
        private readonly Fixture _fixture;
        private readonly Mock<IDurableOrchestrationContext> _context;
        private readonly Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>> _logger;
        private readonly IList<Models.EmploymentCheck> _employmentChecks;
        private readonly IList<LearnerNiNumber> _learnerNiNumbers;
        private readonly IList<EmployerPayeSchemes> _employerPayeSchemes;

        public WhenRunningCreateEmploymentCheckCacheRequestsOrchestrator()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>>();
            _employmentChecks = new List<Models.EmploymentCheck> { _fixture.Create<Models.EmploymentCheck>() };
            _learnerNiNumbers = new List<LearnerNiNumber> { _fixture.Create<LearnerNiNumber>() };
            _employerPayeSchemes = new List<EmployerPayeSchemes> { _fixture.Create<EmployerPayeSchemes>() };
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            _context.Setup(a => a.CallActivityAsync<IList<Models.EmploymentCheck>>(nameof(GetEmploymentChecksBatchActivity), It.IsAny<object>()))
                .ReturnsAsync(_employmentChecks);
            _context.Setup(a => a.CallActivityAsync<IList<LearnerNiNumber>>(nameof(GetLearnerNiNumbersActivity), It.IsAny<IList<Models.EmploymentCheck>>()))
                .ReturnsAsync(_learnerNiNumbers);
            _context.Setup(a => a.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), It.IsAny<IList<Models.EmploymentCheck>>()))
                .ReturnsAsync(_employerPayeSchemes);

            _context.Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestsActivity), It.IsAny<EmploymentCheckData>()));

            // Act
            await sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<IList<Models.EmploymentCheck>>(nameof(GetEmploymentChecksBatchActivity), It.IsAny<object>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync<IList<LearnerNiNumber>>(nameof(GetLearnerNiNumbersActivity), It.IsAny<IList<Models.EmploymentCheck>>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), It.IsAny<IList<Models.EmploymentCheck>>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestsActivity), It.IsAny<EmploymentCheckData>()), Times.Once);
        }
    }
}