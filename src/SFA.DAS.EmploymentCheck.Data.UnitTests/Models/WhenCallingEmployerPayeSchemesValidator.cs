using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Collections.Generic;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Models
{
    public class WhenCallingEmployerPayeSchemesValidator
    {
        const string PAYENotFound = "PAYENotFound";
        const string PAYEFailure = "PAYEFailure";

        private Fixture _fixture;
        private IEmployerPayeSchemesValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _sut = new EmployerPayeSchemesValidator();
        }

        [Test]
        public void When_EmployerPayeSchemes_Is_Null_Return_PAYENotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmploymentCheck, _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create())
                .With(ecd => ecd.EmployerPayeSchemes, () => null)
                .Create();

            // Act
            var result = _sut.PayeSchemesHasError(employmentCheckData);

            // Assert
            result.Should().Be(PAYENotFound);
        }

        [Test]
        public void When_EmployerPayeSchemes_PayeScheme_Is_Empty_Return_PAYENotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmploymentCheck, _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create())
                .With(ecd => ecd.EmployerPayeSchemes, new EmployerPayeSchemes(1, HttpStatusCode.NoContent, null))
                .Create();

            // Act
            var result = _sut.PayeSchemesHasError(employmentCheckData);

            // Assert
            result.Should().Be(PAYENotFound);
        }

        [Test]
        public void When_EmployerPayeSchemes_Status_Is_NotFound_Return_PAYENotFound()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmploymentCheck, _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create())
                .With(ecd => ecd.EmployerPayeSchemes, _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, HttpStatusCode.NotFound).Create())
                .Create();

            // Act
            var result = _sut.PayeSchemesHasError(employmentCheckData);

            // Assert
            result.Should().Be(PAYENotFound);
        }

        [Test]
        public void When_EmployerPayeSchemes_Status_Is_Between_400_And_599__Return_PAYEFailure()
        {
            for (var i = 400; i <= 599; ++i)
            {
                if (i == 404) // skip the NotFound which returns PAYENotFound instead of PAYEFailure
                    continue;

                // Arrange
                var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                    .With(ecd => ecd.EmploymentCheck, _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create())
                    .With(ecd => ecd.EmployerPayeSchemes, _fixture.Build<EmployerPayeSchemes>().With(x => x.HttpStatusCode, (HttpStatusCode)i).Create())
                    .Create();

                // Act
                var result = _sut.PayeSchemesHasError(employmentCheckData);

                // Assert
                result.Should().Be(PAYEFailure);
            }
        }

        [Test]
        public void When_EmployerPayeSchemes_IsValid_Return_Null()
        {
            // Arrange
            var employmentCheckData = _fixture.Build<EmploymentCheckData>()
                .With(ecd => ecd.EmploymentCheck, _fixture.Build<Data.Models.EmploymentCheck>().Without(x => x.ErrorType).Create())
                .Create();

            // Act
            var result = _sut.PayeSchemesHasError(employmentCheckData);

            // Assert
            result.Should().BeNull();
        }
    }
}