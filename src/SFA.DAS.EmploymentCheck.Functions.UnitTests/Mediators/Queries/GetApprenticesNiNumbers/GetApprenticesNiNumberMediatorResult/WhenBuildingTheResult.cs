using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesNiNumbers.GetApprenticesNiNumberMediatorResult
{
    public class WhenBuildingTheResult
    {
        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            // Arrange
            var apprenticeNiNumber = new LearnerNiNumber(1000001, "1000001");

            // Act
            var result = new Functions.Mediators.Queries.GetNiNumbers.GetNiNumbersQueryResult(apprenticeNiNumber);

            // Assert
            Assert.AreEqual(apprenticeNiNumber, result.LearnerNiNumber);
        }
    }
}