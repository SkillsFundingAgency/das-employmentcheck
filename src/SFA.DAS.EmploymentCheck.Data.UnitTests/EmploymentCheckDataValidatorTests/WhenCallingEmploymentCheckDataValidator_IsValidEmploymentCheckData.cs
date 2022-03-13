using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.EmploymentCheckDataValidatorTests
{
    public class WhenCallingEmploymentCheckDataValidator_IsValidEmploymentCheckData
    {
        const string NinoFailure = "NinoFailure";
        const string PAYEFailure = "PAYEFailure";

        private Fixture _fixture;
        private IEmploymentCheckDataValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _sut = new EmploymentCheckDataValidator();
        }

        [Test]
        public void When_IsValidNino_IsNotValid_And_IsValidPayeScheme_IsValid_Return_False_NinoFailure()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, () => null)
                .Create();

            // Act
            var result = _sut.IsValidEmploymentCheckData(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(NinoFailure, result.ErrorType);
        }

        [Test]
        public void When_IsValidNino_IsValid_And_IsValidPayeScheme_IsNotValid_Return_False_PAYEFailure()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmployerPayeSchemes, () => null)
                .Create();

            // Act
            var result = _sut.IsValidEmploymentCheckData(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(PAYEFailure, result.ErrorType);
        }

        [Test]
        public void When_IsValidNino_IsNotValid_And_IsValidPayeScheme_IsNotValid_Return_False_NinoFailureAndPAYEFailure()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, () => null)
                .With(ecd => ecd.EmployerPayeSchemes, () => null)
                .Create();

            // Act
            var result = _sut.IsValidEmploymentCheckData(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(NinoFailure + "And" + PAYEFailure, result.ErrorType);
        }

        [Test]
        public void When_IsValidNino_IsValid_And_IsValidPayeScheme_IsValid_Return_True_EmptyString()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmploymentCheck, _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create())
                .Create();

            // Act
            var result = _sut.IsValidEmploymentCheckData(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.AreEqual(string.Empty, result.ErrorType);
        }
    }
}