using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.StoreEmploymentCheckResult.CommandTests
{
    public class WhenBuildingTheCommand
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_The_Command_Is_Built_Successfully()
        {
            // Arrange
            var employmentCheckCacheRequest = _fixture.Create<EmploymentCheckCacheRequest>();

            //Act
            var command = new StoreEmploymentCheckResultCommand(employmentCheckCacheRequest);

            //Assert
            Assert.AreEqual(command.EmploymentCheckCacheRequest, employmentCheckCacheRequest);
        }
    }
}