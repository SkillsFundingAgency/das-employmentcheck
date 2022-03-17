using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Models
{
    public class WhenCallingLearnerNiNumberValidator
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
        public void When_LearnerNiNumber_Is_Null_Return_NinoNotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, () => null)
                .Create();

            // Act
            var result = _sut.NinoHasError(employmentCheckData);

            // Assert
            result.Should().Be(NinoNotFound);
        }

        [Test]
        public void When_LearnerNiNumber_NiNumber_Is_Null_Return_NinoNotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.NiNumber, () => null).Create())
                .Create();

            // Act
            var result = _sut.NinoHasError(employmentCheckData);

            // Assert
            result.Should().Be(NinoNotFound);
        }

        [Test]
        public void When_LearnerNiNumber_NiNumber_Length_Is_Less_Than_9_Return_NinoInvalid()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.NiNumber, "1234").Create())
                .Create();

            // Act
            var result = _sut.NinoHasError(employmentCheckData);

            // Assert
            result.Should().Be(NinoInvalid);
        }

        [Test]
        public void When_LearnerNiNumber_IsValid_Return_Null()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>().Create();

            // Act
            var result = _sut.NinoHasError(employmentCheckData);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public void When_LearnerNiNumber_Status_Is_NoContent_Returns_NinoNotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                 .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.HttpStatusCode, HttpStatusCode.NoContent).Create())
                 .Create();

            // Act
            var result = _sut.NinoHasError(employmentCheckData);

            // Assert
            result.Should().Be(NinoNotFound);
        }

        [Test]
        public void When_LearnerNiNumber_Status_Is_Between_400_and_599_Return_NinoFailure()
        {
            // Arrange
            for (var i = 400; i <= 599; ++i)
            {
                var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                    .With(ecd => ecd.ApprenticeNiNumber, _fixture.Build<LearnerNiNumber>().With(ni => ni.HttpStatusCode, (HttpStatusCode)i).Create())
                    .Create();

                // Act
                var result = _sut.NinoHasError(employmentCheckData);

                // Assert
                result.Should().Be(NinoFailure);
            }
        }
    }
}