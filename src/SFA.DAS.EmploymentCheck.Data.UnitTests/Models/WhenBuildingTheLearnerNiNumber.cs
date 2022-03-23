using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Data.UnitTests.Models
{
    public class WhenBuildingTheLearnerNiNumber
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_A_LearnerNiNumber_Is_Created()
        {
            // Arrange
            var expected = _fixture.Create<LearnerNiNumber>();

            // Act
            var result = new LearnerNiNumber(expected.Uln, expected.NiNumber, expected.HttpStatusCode);

            // Assert
            result.Uln.Should().Be(expected.Uln);
            result.NiNumber.Should().Be(expected.NiNumber);
            result.HttpStatusCode.Should().Be(expected.HttpStatusCode);
        }
    }
}