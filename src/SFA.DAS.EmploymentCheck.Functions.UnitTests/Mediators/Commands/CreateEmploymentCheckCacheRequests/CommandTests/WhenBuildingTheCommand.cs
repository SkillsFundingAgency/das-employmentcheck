using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.CreateEmploymentCheckCacheRequests.CommandTests
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
            var employmentCheckData = _fixture.Create<EmploymentCheckData>();

            //Act
            var command = new CreateEmploymentCheckCacheRequestCommand(employmentCheckData);

            //Assert
            Assert.AreEqual(command.EmploymentCheckData, employmentCheckData);
        }
    }
}