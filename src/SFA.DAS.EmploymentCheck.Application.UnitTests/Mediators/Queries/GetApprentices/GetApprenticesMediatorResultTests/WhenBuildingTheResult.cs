using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmploymentCheck;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprentices.GetApprenticesMediatorResultTests
{
    public class WhenBuildingTheResult
    {

        [Test]
        public void Then_The_Result_Is_Built_Correctly()
        {
            // Arrange
            var fixture = new Fixture();
            var employmentCheck = fixture.Create<Models.EmploymentCheck>();

            // Act
            var result = new GetEmploymentCheckQueryResult(employmentCheck);

            // Assert
            Assert.AreEqual(employmentCheck, result.EmploymentCheck);
        }
    }
}