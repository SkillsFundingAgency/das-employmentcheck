using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.CreateEmploymentCheckCacheRequests.ResultTests
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
            var _employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();

            //Act
            var result = new CreateEmploymentCheckCacheRequestCommandResult(_employmentCheckCacheRequest);

            //Assert
            Assert.AreEqual(_employmentCheckCacheRequest, result.EmploymentCheckCacheRequest);
        }
    }
}