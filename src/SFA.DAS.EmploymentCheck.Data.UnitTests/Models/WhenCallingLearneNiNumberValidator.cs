using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.EmploymentCheckDataValidatorTests
{
    public class WhenCallingLearneNiNumberValidator
    {
        const string NinoNotFound = "NinoNotFound";
        const string NinoFailure = "NinoFailure";
        const string NinoInvalid = "NinoInvalid";

        private Fixture _fixture;
        private ILearnerNiNumberValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _sut = new LearnerNiNumberValidator();
        }

        [Test]
        public void When_LearnerNiNumber_Is_Null_Return_False_NinoFailure()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, () => null)
                .Create();

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(NinoFailure, result.ErrorType);
        }

        [Test]
        public void When_LearnerNiNumber_NiNumber_Is_Null_IsValidNino_Returns_NinoNotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.NiNumber, () => null).Create())
                .Create();

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(NinoNotFound, result.ErrorType);
        }

        [Test]
        public void When_LearnerNiNumber_NiNumber_Length_Is_Less_Than_9_IsValidNino_Returns_NinoInvalid()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.NiNumber, "1234").Create())
                .Create();

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(NinoInvalid, result.ErrorType);
        }

        [Test]
        public void When_LearnerNiNumber_IsValid_Return_True_Null()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>().Create();

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.IsValid.Equals(true);
            Assert.IsNull(result.ErrorType);
        }

        [Test]
        public void When_LearnerNiNumber_Status_Is_NotFound_Validation_Returns_NinoNotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                 .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.HttpStatusCode, HttpStatusCode.NotFound).Create())
                 .Create();

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(NinoNotFound, result.ErrorType);
        }

        [Test]
        public void When_LearnerNiNumber_Status_Is_NoContent_Validation_Returns_NinoNotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                 .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.HttpStatusCode, HttpStatusCode.NoContent).Create())
                 .Create();

            // Act
            var result = _sut.IsValidNino(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(NinoNotFound, result.ErrorType);
        }

        [Test]
        public void When_LearnerNiNumber_Status_Is_Between_400_and_599_Validation_Returns_NinoFailure()
        {
            // Arrange
            for (var i = 400; i <= 599; ++i)
            {
                if (i == 404) // skip the NotFound status which returns NinoNotFound instead of NinoFailure
                    continue;

                var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                    .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.HttpStatusCode, (HttpStatusCode)i).Create())
                    .Create();

                // Act
                var result = _sut.IsValidNino(employmentCheckData);

                // Assert
                result.IsValid.Equals(false);
                Assert.AreEqual(NinoFailure, result.ErrorType);
            }
        }
    }
}