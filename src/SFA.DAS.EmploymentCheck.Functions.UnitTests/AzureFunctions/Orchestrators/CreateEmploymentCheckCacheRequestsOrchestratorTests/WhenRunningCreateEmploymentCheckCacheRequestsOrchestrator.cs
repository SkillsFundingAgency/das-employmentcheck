using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Net;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.CreateEmploymentCheckCacheRequestsOrchestratorTests
{
    public class WhenRunningCreateEmploymentCheckCacheRequestsOrchestrator
    {
        private Fixture _fixture;
        private Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>> _logger;
        private Mock<IDurableOrchestrationContext> _context;
        private Mock<IEmploymentCheckDataValidator> _employmentCheckDataValidator;
        private Models.EmploymentCheck _employmentCheck;
        private EmploymentCheckData _employmentCheckData;
        private CreateEmploymentCheckCacheRequestsOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>>();
            _employmentCheckDataValidator = new Mock<IEmploymentCheckDataValidator>();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            _employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmploymentCheck, _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create())
                .With(ni => ni.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(x => x.HttpStatusCode, HttpStatusCode.OK).Create())
                .With(paye => paye.EmployerPayeSchemes, _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.OK).Create())
                .Create();
            _sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object, _employmentCheckDataValidator.Object);
        }

        [Test]
        public async Task Then_The_GetEmploymentCheckActivity_Is_Called()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(_fixture.Create<LearnerNiNumber>());
            var employerPayeSchemesTask = Task.FromResult(_fixture.Create<EmployerPayeSchemes>());
            (bool IsValid, string ErrorType) validStatus = (true, string.Empty);

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), _employmentCheck))
                .Returns(learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), _employmentCheck))
                .Returns(employerPayeSchemesTask);

            _employmentCheckDataValidator
                .Setup(x => x.IsValidEmploymentCheckData(It.IsAny<EmploymentCheckData>()))
                .Returns(validStatus);

            _context.Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), It.IsAny<Object>()))
                .Verifiable();

            _context.Setup(a => a.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), It.IsAny<Object>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null), Times.Once);
        }

        [Test]
        public async Task The_The_GetLearnerNiNumberActivity_IsCalled()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(_fixture.Create<LearnerNiNumber>());
            var employerPayeSchemesTask = Task.FromResult(_fixture.Create<EmployerPayeSchemes>());
            (bool IsValid, string ErrorType) validStatus = (true, string.Empty);

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), _employmentCheck))
                .Returns(learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), _employmentCheck))
                .Returns(employerPayeSchemesTask);

            _employmentCheckDataValidator
                .Setup(x => x.IsValidEmploymentCheckData(It.IsAny<EmploymentCheckData>()))
                .Returns(validStatus);

            _context.Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), It.IsAny<Object>()))
                .Verifiable();

            _context.Setup(a => a.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), It.IsAny<Object>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), _employmentCheck), Times.Once);
        }

        [Test]
        public async Task Then_The_GetEmployerPayeSchemeActivity_IsCalled()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(_fixture.Create<LearnerNiNumber>());
            var employerPayeSchemesTask = Task.FromResult(_fixture.Create<EmployerPayeSchemes>());
            (bool IsValid, string ErrorType) validStatus = (true, string.Empty);

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), _employmentCheck))
                .Returns(learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), _employmentCheck))
                .Returns(employerPayeSchemesTask);

            _employmentCheckDataValidator
                .Setup(x => x.IsValidEmploymentCheckData(It.IsAny<EmploymentCheckData>()))
                .Returns(validStatus);

            _context.Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), It.IsAny<Object>()))
                .Verifiable();

            _context.Setup(a => a.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), It.IsAny<Object>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), _employmentCheck), Times.Once);
        }

        [Test]
        public async Task When_The_EmploymentCheckData_Is_Valid_Then_The_CreateCashRequestActivity_Is_Called()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(_fixture.Create<LearnerNiNumber>());
            var employerPayeSchemesTask = Task.FromResult(_fixture.Create<EmployerPayeSchemes>());
            (bool IsValid, string ErrorType) validStatus = (true, string.Empty);

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), _employmentCheck))
                .Returns(learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), _employmentCheck))
                .Returns(employerPayeSchemesTask);

            _employmentCheckDataValidator
                .Setup(x => x.IsValidEmploymentCheckData(It.IsAny<EmploymentCheckData>()))
                .Returns(validStatus);

            _context.Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), It.IsAny<EmploymentCheckData>()))
                .Verifiable();

            _context.Setup(a => a.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), It.IsAny<EmploymentCheckData>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), It.IsAny<Object>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), It.IsAny<Object>()), Times.Never);
        }

        [Test]
        public async Task When_The_EmploymentCheckData_Is_InValid_Then_The_StoreCompletedEmploymentCheckActivity_Is_Called()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(_fixture.Create<LearnerNiNumber>());
            var employerPayeSchemesTask = Task.FromResult(_fixture.Create<EmployerPayeSchemes>());
            (bool IsValid, string ErrorType) validStatus = (false, string.Empty);

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(nameof(GetEmploymentCheckActivity), null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(nameof(GetLearnerNiNumberActivity), _employmentCheck))
                .Returns(learnerNiNumberTask);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(nameof(GetEmployerPayeSchemesActivity), _employmentCheck))
                .Returns(employerPayeSchemesTask);

            _employmentCheckDataValidator
                .Setup(x => x.IsValidEmploymentCheckData(It.IsAny<EmploymentCheckData>()))
                .Returns(validStatus);

            _context.Setup(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), It.IsAny<EmploymentCheckData>()))
                .Verifiable();

            _context.Setup(a => a.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), It.IsAny<EmploymentCheckData>()))
                .Verifiable();

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync(nameof(StoreCompletedEmploymentCheckActivity), It.IsAny<Object>()), Times.Once);
            _context.Verify(a => a.CallActivityAsync(nameof(CreateEmploymentCheckCacheRequestActivity), It.IsAny<Object>()), Times.Never);
        }
    }
}