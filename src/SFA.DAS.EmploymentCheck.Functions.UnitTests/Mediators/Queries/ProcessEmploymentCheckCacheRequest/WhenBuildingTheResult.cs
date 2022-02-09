using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.ProcessEmploymentCheckCacheRequest;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.ProcessEmploymentCheckCacheRequest
{
    public class WhenBuildingTheResult
    {
        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            //Arrange
            var fixture = new Fixture();
            var employmentCheckCacheRequest = fixture.Create<EmploymentCheckCacheRequest>();

            //Act

            var result = new ProcessEmploymentCheckCacheRequestQueryResult(employmentCheckCacheRequest);

            //Assert

            Assert.AreEqual(employmentCheckCacheRequest, result.EmploymentCheckCacheRequest);
        }
    }
}