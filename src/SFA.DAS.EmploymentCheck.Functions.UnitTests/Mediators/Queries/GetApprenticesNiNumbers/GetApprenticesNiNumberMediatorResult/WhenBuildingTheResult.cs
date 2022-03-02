using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetNiNumber;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesNiNumbers.GetApprenticesNiNumberMediatorResult
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
            var apprenticeNiNumber = _fixture.Create<LearnerNiNumber>();

            // Act
            var result = new GetNiNumberQueryResult(apprenticeNiNumber);

            // Assert
            Assert.AreEqual(apprenticeNiNumber, result.LearnerNiNumber);
        }
    }
}