using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.CreateEmploymentCheckCacheRequestOrchestratorTests
{
    public class WhenRunningCreateEmploymentCheckCacheRequestOrchestrator
    {
        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Models.EmploymentCheck _employmentCheck;
        private LearnerNiNumber _learnerNiNumber;
        private IList<EmployerPayeSchemes> _employerPayeSchemes;
        private CreateEmploymentCheckCacheRequestOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();
            _learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            _employerPayeSchemes = _fixture.CreateMany<EmployerPayeSchemes>(1).ToList();
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
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumbersActivity), _employmentCheck))
                .ReturnsAsync(_learnerNiNumber);

            _context
                .Setup(a => a.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), _employmentCheck))
                .ReturnsAsync(_employerPayeSchemes);

            // Act
            await _sut.CreateEmploymentCheckCacheRequestTask(_context.Object, null);

            // Assert
            _context.Verify(a => a.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumbersActivity), _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync<IList<EmployerPayeSchemes>>(nameof(GetEmployerPayeSchemesActivity), _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestsActivity), 
                It.Is<EmploymentCheckData>(ecd => 
                    Equals(ecd.ApprenticeNiNumber, _learnerNiNumber)
                    && Equals(ecd.EmployerPayeSchemes, _employerPayeSchemes)
                    && ecd.EmploymentCheck == _employmentCheck
                    )
                ), Times.Once);
        }
    }
}