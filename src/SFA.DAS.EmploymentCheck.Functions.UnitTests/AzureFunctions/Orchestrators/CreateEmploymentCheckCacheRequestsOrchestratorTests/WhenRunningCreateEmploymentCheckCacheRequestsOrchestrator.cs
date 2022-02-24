using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.CreateEmploymentCheckCacheRequestsOrchestratorTests
{
    public class WhenRunningCreateEmploymentCheckCacheRequestsOrchestrator
    {
        private string _checkActivityName = nameof(GetEmploymentCheckActivity);
        private string _ninoActivityName = nameof(GetLearnerNiNumberActivity);
        private string _payeActivityName = nameof(GetEmployerPayeSchemesActivity);
        private string _requestActivityName = nameof(CreateEmploymentCheckCacheRequestActivity);

        private Fixture _fixture;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>> _logger;

        private Models.EmploymentCheck _employmentCheck;
        private Task<LearnerNiNumber> _learnerNiNumberTask;
        private Task<EmployerPayeSchemes> _employerPayeSchemesTask;
        private EmploymentCheckData _employmentCheckData;

        private CreateEmploymentCheckCacheRequestsOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>>();
            _employmentCheck = _fixture.Create<Models.EmploymentCheck>();
            _learnerNiNumberTask = _fixture.Create<Task<LearnerNiNumber>>();
            _employerPayeSchemesTask = _fixture.Create<Task<EmployerPayeSchemes>>();
            _employmentCheckData = new EmploymentCheckData(_employmentCheck, _learnerNiNumberTask.Result, _employerPayeSchemesTask.Result);

            _sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);
        }

        [Test]
        public async Task Then_The_Activities_Are_Called()
        {
            // Arrange
            _context
                .Setup(a => a.CallActivityAsync<Models.EmploymentCheck>(_checkActivityName, null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck))
                .Returns(_learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck))
                .Returns(_employerPayeSchemesTask);

            _context.Setup(a => a.CallActivityAsync(_requestActivityName, It.IsAny<Object>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Models.EmploymentCheck>(_checkActivityName, null), Times.Once);
            _context.Verify(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync(_requestActivityName, It.IsAny<Object>()), Times.Once);
        }
    }
}