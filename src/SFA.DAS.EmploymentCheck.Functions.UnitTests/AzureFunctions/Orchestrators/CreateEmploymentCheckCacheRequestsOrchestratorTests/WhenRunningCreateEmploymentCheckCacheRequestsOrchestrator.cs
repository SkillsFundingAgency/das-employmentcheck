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
        const string NinoNotFound = "NinoNotFound";
        const string NinoInvalid = "NinoInvalid";
        const string PayeNotFound = "PAYENotFound";
        const string PayeFailure = "PAYEFailure";

        private readonly string _checkActivityName = nameof(GetEmploymentCheckActivity);
        private readonly string _ninoActivityName = nameof(GetLearnerNiNumberActivity);
        private readonly string _payeActivityName = nameof(GetEmployerPayeSchemesActivity);
        private readonly string _requestActivityName = nameof(CreateEmploymentCheckCacheRequestActivity);
        private readonly string _storeActivityName = nameof(StoreCompletedEmploymentCheckActivity);

        private Fixture _fixture;
        private Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>> _logger;
        private Mock<IDurableOrchestrationContext> _context;
        private Models.EmploymentCheck _employmentCheck;
        private CreateEmploymentCheckCacheRequestsOrchestrator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _context = new Mock<IDurableOrchestrationContext>();
            _logger = new Mock<ILogger<CreateEmploymentCheckCacheRequestsOrchestrator>>();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
            _sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);
        }

        [Test]
        public async Task When_The_CreateCashRequestActivity_Is_Called_And_There_Is_An_EmploymentCheck_It_Is_Returned()
        {
            // Arrange
            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null))
                .ReturnsAsync(_employmentCheck);

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null), Times.Once);
        }

        [Test]
        public async Task When_The_CreateCashRequestActivity_Is_Called_And_An_Exception_Is_Returned()
        {
            // Arrange
            var exception = new Exception("An error occurred");

               _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null))
                .ThrowsAsync(exception);

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null), Times.Once);
        }

        [Test]
        public async Task When_There_Is_An_EmploymentCheck_The_GetLearnerNiActivity_IsCalled()
        {
            // Arrange
            var learnerNiNumberTask = Task.FromResult(_fixture.Create<LearnerNiNumber>());

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck))
                .Returns(learnerNiNumberTask);

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<LearnerNiNumber>(_ninoActivityName, _employmentCheck), Times.Once);
        }

        [Test]
        public async Task When_There_Is_An_EmploymentCheck_The_GetEmployerPayeSchemeActivity_IsCalled()
        {
            // Arrange
            var employerPayeSchemesTask = Task.FromResult(_fixture.Create<EmployerPayeSchemes>());

            _context
                .Setup(a => a.CallActivityAsync<Data.Models.EmploymentCheck>(_checkActivityName, null))
                .ReturnsAsync(_employmentCheck);

            _context
                .Setup(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck))
                .Returns(employerPayeSchemesTask);

            // Act
            await _sut.CreateEmploymentCheckRequestsTask(_context.Object);

            // Assert
            _context.Verify(a => a.CallActivityAsync<EmployerPayeSchemes>(_payeActivityName, _employmentCheck), Times.Once);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Valid_IndividualPayeSchemeValidation_Returns_True()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            LearnerNiNumber learnerNiNumber = null;
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = string.Empty;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IndividualPayeSchemeValidation(employmentCheckData, PayeNotFound, isValidPayeScheme, existingError);

            // Assert
            result.Equals(true);
            Assert.AreEqual(string.Empty, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Null_IndividualPayeSchemeValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string> { "" } };
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = string.Empty;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IndividualPayeSchemeValidation(employmentCheckData, PayeNotFound, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PayeNotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Null_With_Existing_Nino_Error_IndividualPayeSchemeValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string> { "" } };
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = employmentCheck.ErrorType;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IndividualPayeSchemeValidation(employmentCheckData, PayeNotFound, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PayeNotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }


        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Valid_PayeSchemeValueValidation_Returns_True()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            LearnerNiNumber learnerNiNumber = null;
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.OK).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = string.Empty;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual(string.Empty, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Null_PayeSchemeValueValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = null;
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = string.Empty;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PayeFailure, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Empty_StatusOK_PayeSchemeValueValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string>(), HttpStatusCode = HttpStatusCode.OK };
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = string.Empty;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PayeNotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Empty_With_Existing_Nino_Error_StatusOK_PayeSchemeValueValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string>(), HttpStatusCode = HttpStatusCode.OK };
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = employmentCheck.ErrorType;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PayeNotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Empty_StatusNotFound_PayeSchemeValueValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string>(), HttpStatusCode = HttpStatusCode.NotFound };
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = employmentCheck.ErrorType;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PayeNotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Empty_With_Existing_Nino_Error_StatusNotFound_PayeSchemeValueValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string>(), HttpStatusCode = HttpStatusCode.NotFound };
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = string.Empty;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PayeNotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Empty_StatusNoContent_PayeSchemeValueValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string>(), HttpStatusCode = HttpStatusCode.NoContent };
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = string.Empty;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PayeFailure, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_An_Individual_EmployerPayeScheme_Is_Empty_With_Existing_Nino_Error_StatusNoContent_PayeSchemeValueValidation_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes { EmployerAccountId = 1, PayeSchemes = new List<string>(), HttpStatusCode = HttpStatusCode.NoContent };
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            bool isValidPayeScheme = true;
            string existingError = employmentCheck.ErrorType;

            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.PayeSchemeValueValidation(employmentCheckData, PayeNotFound, PayeFailure, isValidPayeScheme, existingError);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PayeFailure}", employmentCheckData.EmploymentCheck.ErrorType);
        }


        [Test]
        public void When_The_Nino_Is_Valid_IsValidNino_Returns_True()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(true);
            Assert.AreEqual(string.Empty, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_The_Nino_Is_Null_IsValidNino_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            LearnerNiNumber learnerNiNumber = null;
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual("NinoNotFound", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_The_Nino_Length_Is_Too_Short_IsValidNino_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            var learnerNiNumber = _fixture.Build<LearnerNiNumber>().With(x => x.NiNumber, "1234").Create();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual("NinoInvalid", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_The_PayeScheme_Is_Valid_IsValidPayeScheme_Returns_True()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.OK).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(true);
            Assert.AreEqual(string.Empty, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_The_PayeScheme_Is_Null_IsValidPayeScheme_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = null;
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual("PAYEFailure", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_The_PayeScheme_Is_NotNull_And_Response_NotFound_IsValidPayeScheme_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NotFound).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual("PAYENotFound", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_The_PayeScheme_Is_NotNull_And_Response_NotOk_Or_NotFound_IsValidPayeScheme_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, string.Empty).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NotModified).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual("PAYEFailure", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_NinoNotFound_Error_And_The_PayeScheme_Is_Null_IsValidPayeScheme_Returns_False()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, "NinoNotFound").Create();
            LearnerNiNumber learnerNiNumber = null;
            EmployerPayeSchemes payeScheme = null;
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual("NinoNotFoundAndPAYEFailure", employmentCheckData.EmploymentCheck.ErrorType);
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