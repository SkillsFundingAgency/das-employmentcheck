using AutoFixture;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureFunctions.Orchestrators.CreateEmploymentCheckCacheRequestsOrchestratorTests
{
    public class WhenRunningCreateEmploymentCheckCacheRequestsOrchestrator
    {
        const string NinoNotFound = "NinoNotFound";
        const string NinoFailure = "NinoFailure";
        const string NinoInvalid = "NinoInvalid";
        const string PAYENotFound = "PAYENotFound";
        const string PAYEFailure = "PAYEFailure";

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
        public void When_There_Is_A_LearnerNiNumber_And_It_Is_Null_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoFailure()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            LearnerNiNumber learnerNiNumber = null;
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoFailure, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Nino_Is_Null_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoNotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = new LearnerNiNumber(1, null, HttpStatusCode.NoContent);
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoNotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Status_Is_NoContent_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoNotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Build<LearnerNiNumber>().With(x => x.HttpStatusCode, HttpStatusCode.NoContent).Create();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoNotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Status_Is_Between_400_and_599_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoFailure()
        {
            // Arrange
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            for(var i = 400; i <= 599; ++i)
            {
                if (i == 404) // skip the NotFound which returns NinoNotFound instead of NinoFailure
                    continue;

                var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
                var learnerNiNumber = _fixture.Build<LearnerNiNumber>().With(x => x.HttpStatusCode, (HttpStatusCode)i).Create();
                var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

                // Act
                var result = sut.IsValidNino(employmentCheckData);

                // Assert
                result.Equals(false);
                Assert.AreEqual(NinoFailure, employmentCheckData.EmploymentCheck.ErrorType);
            }
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_The_Nino_Length_Is_Less_Than_9_IsValidNino_Returns_False_And_ErrorType_Is_Set_To_NinoInvalid()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Build<LearnerNiNumber>().With(x => x.NiNumber, "1234").Create();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(NinoInvalid, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_A_LearnerNiNumber_And_It_Has_A_Valid_Nino_IsValidNino_Returns_True_And_ErrorType_Is_Set_To_Null()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidNino(employmentCheckData);

            // Assert
            result.Equals(true);
            Assert.AreEqual(null, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_It_Is_Null_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYEFailure()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = null;
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYEFailure, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_It_Is_Null_And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYEFailure()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = null;
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYEFailure}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_PayeScheme_Is_Null_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes(1, HttpStatusCode.NoContent, null);
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYENotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_PayeScheme_Is_Null_And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            EmployerPayeSchemes payeScheme = new EmployerPayeSchemes(1, HttpStatusCode.NoContent, null);
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYENotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_NotFound_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NotFound).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYENotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_NotFound_And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NotFound).Create();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYENotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_Between_400_And_599__And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYEFailure()
        {
            // Arrange
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            for(var i = 400; i <= 599; ++i)
            {
                if (i == 404) // skip the NotFound which returns PAYENotFound instead of PAYEFailure
                    continue;

                var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
                var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, (HttpStatusCode)i).Create();
                var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

                // Act
                var result = sut.IsValidPayeScheme(employmentCheckData);

                // Assert
                result.Equals(false);
                Assert.AreEqual(PAYEFailure, employmentCheckData.EmploymentCheck.ErrorType);
            }
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_Status_Is_Between_400_And_599__And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYEFailure()
        {
            // Arrange
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            for (var i = 400; i <= 599; ++i)
            {
                if (i == 404) // skip the NotFound which returns PAYENotFound instead of PAYEFailure
                    continue;

                var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
                var payeScheme = _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, (HttpStatusCode)i).Create();
                var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);

                // Act
                var result = sut.IsValidPayeScheme(employmentCheckData);

                // Assert
                result.Equals(false);
                Assert.AreEqual($"{NinoNotFound}And{PAYEFailure}", employmentCheckData.EmploymentCheck.ErrorType);
            }
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_PayeScheme_Has_An_Empty_PayeScheme_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_PAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = new EmployerPayeSchemes(1, HttpStatusCode.OK, new List<string> { { "Paye" }, { "" } });
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(PAYENotFound, employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_The_PayeScheme_Has_An_Empty_PayeScheme_And_ErrorType_Is_NinoNotFound_IsValidPayeScheme_Returns_False_And_ErrorType_Is_Set_To_NinoNotFoundAndPAYENotFound()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().With(x => x.ErrorType, NinoNotFound).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = new EmployerPayeSchemes(1, HttpStatusCode.OK, new List<string> { { "Paye" }, { "" } });
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual($"{NinoNotFound}And{PAYENotFound}", employmentCheckData.EmploymentCheck.ErrorType);
        }

        [Test]
        public void When_There_Is_An_EmployerPayeSchemes_And_ErrorType_Is_Empty_IsValidPayeScheme_Returns_True_And_ErrorType_Is_Set_To_Null()
        {
            // Arrange
            var employmentCheck = _fixture.Build<Models.EmploymentCheck>().Without(x => x.ErrorType).Create();
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();
            var employmentCheckData = new EmploymentCheckData(employmentCheck, learnerNiNumber, payeScheme);
            var sut = new CreateEmploymentCheckCacheRequestsOrchestrator(_logger.Object);

            // Act
            var result = sut.IsValidPayeScheme(employmentCheckData);

            // Assert
            result.Equals(false);
            Assert.AreEqual(null, employmentCheckData.EmploymentCheck.ErrorType);
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
            var learnerNiNumberTask = Task.FromResult(new LearnerNiNumber(1, "123456789", HttpStatusCode.OK));
            var employerPayeSchemesTask = Task.FromResult(new EmployerPayeSchemes(1, HttpStatusCode.OK, new List<string> { "paye" }));

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
            var learnerNiNumberTask = Task.FromResult(new LearnerNiNumber(1, "", HttpStatusCode.NoContent));
            var employerPayeSchemesTask = Task.FromResult(new EmployerPayeSchemes(1, HttpStatusCode.OK, new List<string> { "paye" }));

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
            var learnerNiNumberTask = Task.FromResult(new LearnerNiNumber(1, "123456789", HttpStatusCode.OK));
            var employerPayeSchemesTask = Task.FromResult(new EmployerPayeSchemes(1, HttpStatusCode.NotFound, null));

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