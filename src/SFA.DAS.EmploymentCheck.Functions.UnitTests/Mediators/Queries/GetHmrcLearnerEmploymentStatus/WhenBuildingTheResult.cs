using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetHmrcLearnerEmploymentStatus;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetHmrcLearnerEmploymentStatus
{
    public class WhenBuildingTheResult
    {

        [Test]
        public void Then_The_Result_Is_Built_Correctly()
        {
            // Arrange
            var fixture = new Fixture();
            var employmentCheckCacheRequest = fixture.Create<Models.EmploymentCheckCacheRequest>();

            // Act
            var result = new GetHmrcLearnerEmploymentStatusQueryResult(employmentCheckCacheRequest);

            // Assert
            Assert.AreEqual(employmentCheckCacheRequest, result.EmploymentCheckCacheRequest);
        }
    }
}