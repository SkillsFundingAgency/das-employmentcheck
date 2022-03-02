using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprentices.GetApprenticesMediatorResultTests
{
    public class WhenBuildingTheResult
    {

        [Test]
        public void Then_The_Result_Is_Built_Correctly()
        {
            // Arrange
            var fixture = new Fixture();
            var employmentCheck = fixture.Create<Data.Models.EmploymentCheck>();

            // Act
            var result = new GetEmploymentCheckQueryResult(employmentCheck);

            // Assert
            Assert.AreEqual(employmentCheck, result.EmploymentCheck);
        }
    }
}