using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetDbLearnerNiNumbers
{
    public class WhenBuildingTheResult
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            // Arrange
            var learnerNiNumber = _fixture.Create<LearnerNiNumber>();

            // Act
            var result = new GetDbNiNumberQueryResult(learnerNiNumber);

            // Assert
            Assert.AreEqual(learnerNiNumber, result.LearnerNiNumber);
        }
    }
}