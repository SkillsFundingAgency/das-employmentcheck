using AutoFixture;
using FluentAssertions;
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
        public void When_Nino_IsNotValid_And_PayeScheme_IsValid_Return_NinoFailure()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, () => null)
                .Create();

            _learnerNiNumberValidatorMock.Setup(x => x.NinoHasError(It.IsAny<EmploymentCheckData>()))
                .Returns(NinoFailure);

            _employerPayeSchemesMock.Setup(x => x.PayeSchemesHasError(It.IsAny<EmploymentCheckData>()))
                .Returns( () => null );

            // Act
            var result = _sut.EmploymentCheckDataHasError(employmentCheckData);

            // Assert
            result.Should().Be(NinoFailure);
        }

        [Test]
        public void When_Nino_IsValid_And_PayeScheme_IsNotValid_Return_False_PAYEFailure()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmployerPayeSchemes, () => null)
                .Create();

            _learnerNiNumberValidatorMock.Setup(x => x.NinoHasError(It.IsAny<EmploymentCheckData>()))
                .Returns( () => null );

            _employerPayeSchemesMock.Setup(x => x.PayeSchemesHasError(It.IsAny<EmploymentCheckData>()))
                .Returns(PAYEFailure);

            // Act
            var result = _sut.EmploymentCheckDataHasError(employmentCheckData);

            // Assert
            result.Should().Be(PAYEFailure);
        }

        [Test]
        public void When_Nino_IsNotValid_And_PayeScheme_IsNotValid_Return_NinoAndPAYENotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, () => null)
                .With(ecd => ecd.EmployerPayeSchemes, () => null)
                .Create();

            _learnerNiNumberValidatorMock.Setup(x => x.NinoHasError(It.IsAny<EmploymentCheckData>()))
                .Returns(NinoFailure);

            _employerPayeSchemesMock.Setup(x => x.PayeSchemesHasError(It.IsAny<EmploymentCheckData>()))
                .Returns(PAYEFailure);

            // Act
            var result = _sut.EmploymentCheckDataHasError(employmentCheckData);

            // Assert
            result.Should().Be("NinoAndPAYENotFound");
        }

        [Test]
        public void When_Nino_IsValid_And_PayeScheme_IsValid_Return_Null()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmploymentCheck, _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create())
                .Create();

            _learnerNiNumberValidatorMock.Setup(x => x.NinoHasError(It.IsAny<EmploymentCheckData>()))
                .Returns( () => null );

            _employerPayeSchemesMock.Setup(x => x.PayeSchemesHasError(It.IsAny<EmploymentCheckData>()))
                .Returns( () => null );

            // Act
            var result = _sut.EmploymentCheckDataHasError(employmentCheckData);

            // Assert
            result.Should().BeNull();
        }
    }
}