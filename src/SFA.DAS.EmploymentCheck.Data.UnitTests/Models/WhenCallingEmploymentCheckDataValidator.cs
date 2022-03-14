using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Models
{
    public class WhenCallingEmploymentCheckDataValidator
    {
        const string NinoFailure = "NinoFailure";
        const string PAYEFailure = "PAYEFailure";

        private Fixture _fixture;
        private Mock<ILearnerNiNumberValidator> _learnerNiNumberValidatorMock;
        private Mock<IEmployerPayeSchemesValidator> _employerPayeSchemesMock;
        private IEmploymentCheckDataValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _learnerNiNumberValidatorMock = new Mock<ILearnerNiNumberValidator>();
            _employerPayeSchemesMock = new Mock<IEmployerPayeSchemesValidator>();
            _sut = new EmploymentCheckDataValidator(_learnerNiNumberValidatorMock.Object, _employerPayeSchemesMock.Object);
        }

        [Test]
        public void When_IsValidNino_IsNotValid_And_IsValidPayeScheme_IsValid_Return_False_NinoFailure()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, () => null)
                .Create();

            _learnerNiNumberValidatorMock.Setup(x => x.IsValidNino(It.IsAny<EmploymentCheckData>()))
                .Returns((false, NinoFailure));

            _employerPayeSchemesMock.Setup(x => x.IsValidPayeScheme(It.IsAny<EmploymentCheckData>()))
                .Returns((true, null));

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

            _learnerNiNumberValidatorMock.Setup(x => x.IsValidNino(It.IsAny<EmploymentCheckData>()))
                .Returns((true, null));

            _employerPayeSchemesMock.Setup(x => x.IsValidPayeScheme(It.IsAny<EmploymentCheckData>()))
                .Returns((false, PAYEFailure));

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

            _learnerNiNumberValidatorMock.Setup(x => x.IsValidNino(It.IsAny<EmploymentCheckData>()))
                .Returns((false, NinoFailure));

            _employerPayeSchemesMock.Setup(x => x.IsValidPayeScheme(It.IsAny<EmploymentCheckData>()))
                .Returns((false, PAYEFailure));

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

            _learnerNiNumberValidatorMock.Setup(x => x.IsValidNino(It.IsAny<EmploymentCheckData>()))
                .Returns((true, null));

            _employerPayeSchemesMock.Setup(x => x.IsValidPayeScheme(It.IsAny<EmploymentCheckData>()))
                .Returns((true, null));


            // Act
            var result = _sut.IsValidEmploymentCheckData(employmentCheckData);

            // Assert
            result.IsValid.Equals(false);
            Assert.IsNull(result.ErrorType);
        }
    }
}