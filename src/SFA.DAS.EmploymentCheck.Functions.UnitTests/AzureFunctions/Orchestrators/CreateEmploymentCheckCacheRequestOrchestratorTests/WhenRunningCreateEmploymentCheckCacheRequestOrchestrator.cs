using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.CreateEmploymentCheckCacheRequestOrchestratorTests
{
    public class WhenRunningCreateEmploymentCheckCacheRequestOrchestrator
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Models.EmploymentCheck _employmentCheck;
        private IList<LearnerNiNumber> _learnerNiNumbers;
        private IList<EmployerPayeSchemes> _employerPayeSchemes;
        private CreateEmploymentCheckCacheRequestOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();

            _learnerNiNumbers = new List<LearnerNiNumber>
            {
                _fixture.Create<LearnerNiNumber>()
            };

            _employerPayeSchemes = new List<EmployerPayeSchemes>
            {
                _fixture.Create<EmployerPayeSchemes>()
            };

            _sut = new CreateEmploymentCheckCacheRequestOrchestrator();
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            _context
                .Setup(i => i.GetInput<Models.EmploymentCheck>())
                .Returns(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<IList<LearnerNiNumber>>(nameof(GetLearnerNiNumbersActivity), It.IsAny<IList<Models.EmploymentCheck>>()))
                .ReturnsAsync(_learnerNiNumbers);

            _context
                .Setup(a => a.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), It.IsAny<IList<Models.EmploymentCheck>>()))
                .ReturnsAsync(_employerPayeSchemes);

            _context
                .Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestsActivity), It.IsAny<EmploymentCheckData>()));

            // Act
            await _sut.CreateEmploymentCheckCacheRequestTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<IList<LearnerNiNumber>>(nameof(GetLearnerNiNumbersActivity), It.IsAny<IList<Models.EmploymentCheck>>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), It.IsAny<IList<Models.EmploymentCheck>>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(CreateEmploymentCheckCacheRequestsActivity), It.IsAny<EmploymentCheckData>()), Times.Once);
        }
    }
}