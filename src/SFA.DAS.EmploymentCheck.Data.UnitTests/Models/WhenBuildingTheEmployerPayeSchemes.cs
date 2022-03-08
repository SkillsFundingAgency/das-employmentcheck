using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Models
{
    public class WhenBuildingTheEmployerPayeSchemes
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_An_EmployerPayeSchemes_Is_Created()
        {
            // Arrange
            var expected = _fixture.Create<EmployerPayeSchemes>();

            // Act
            var result = new EmployerPayeSchemes(expected.EmployerAccountId, expected.PayeSchemes, expected.HttpStatusCode);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}