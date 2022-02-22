using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Linq;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetDbLearnerNiNumbers.GetDbLearnerNiNumberMediatorResult
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
            var learnerNiNumbers = _fixture.CreateMany<LearnerNiNumber>().ToList();

            // Act
            var result = new Functions.Mediators.Queries.GetNiNumbers.GetNiNumbersQueryResult(learnerNiNumbers);

            // Assert
            Assert.AreEqual(learnerNiNumbers, result.LearnerNiNumber);
        }
    }
}