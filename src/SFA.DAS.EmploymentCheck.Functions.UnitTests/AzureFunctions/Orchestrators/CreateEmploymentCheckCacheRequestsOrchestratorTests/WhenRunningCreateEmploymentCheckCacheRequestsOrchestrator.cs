using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.CreateEmploymentCheckCacheRequestsOrchestratorTests
{
    public class WhenRunningCreateEmploymentCheckCacheRequestsOrchestrator
    {
        private readonly string _checkActivityName = nameof(GetEmploymentCheckActivity);
        private readonly string _ninoActivityName = nameof(GetLearnerNiNumberActivity);
        private readonly string _payeActivityName = nameof(GetEmployerPayeSchemesActivity);
        private readonly string _requestActivityName = nameof(CreateEmploymentCheckCacheRequestActivity);
        private readonly string _storeActivityName = nameof(StoreCompletedEmploymentCheckActivity);

        private Fixture _fixture;
        private Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>> _logger;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<IEmploymentCheckService> _employmentCheckService;
        private Models.EmploymentCheck _employmentCheck;
        private CreateEmploymentCheckCacheRequestsOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>>();
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            _sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object, _employmentCheckService.Object);
        }

        [Test]
        public async Task When_The_EmploymentCheckData_Is_Valid_Then_The_CreateCashRequestActivity_Is_Called()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(new LearnerNiNumber { Uln = 1, NiNumber = "123456789" });
            var employerPayeSchemesTask = Task.FromResult(new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string> { "paye" }, HttpStatusCode = HttpStatusCode.OK });

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck))
                .Returns(learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck))
                .Returns(employerPayeSchemesTask);

            _context.Setup(a => a.CallActivityAsync(_requestActivityName, It.IsAny<Object>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null), Times.Once);
            _context.Verify(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync(_requestActivityName, It.IsAny<Object>()), Times.Once);
        }

        [Test]
        public async Task When_The_EmploymentCheckData_Nino_Is_InValid_Then_The_StoreCompletedEmploymentCheckActivity_Is_Called()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(new LearnerNiNumber());
            var employerPayeSchemesTask = Task.FromResult(new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string> { "paye" }, HttpStatusCode = HttpStatusCode.OK });

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck))
                .Returns(learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck))
                .Returns(employerPayeSchemesTask);

            _context.Setup(a => a.CallActivityAsync(_requestActivityName, It.IsAny<Object>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null), Times.Once);
            _context.Verify(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync(_storeActivityName, It.IsAny<Object>()), Times.Once);
        }

        [Test]
        public async Task When_The_EmploymentCheckData_PayeScheme_Is_InValid_Then_The_StoreCompletedEmploymentCheckActivity_Is_Called()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(new LearnerNiNumber { Uln = 1, NiNumber = "123456789" });
            var employerPayeSchemesTask = Task.FromResult(new EmployerPayeSchemes());

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck))
                .Returns(learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck))
                .Returns(employerPayeSchemesTask);

            _context.Setup(a => a.CallActivityAsync(_requestActivityName, It.IsAny<Object>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null), Times.Once);
            _context.Verify(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck), Times.Once);
            _context.Verify(a => a.CallActivityAsync(_storeActivityName, It.IsAny<Object>()), Times.Once);
        }
    }
}