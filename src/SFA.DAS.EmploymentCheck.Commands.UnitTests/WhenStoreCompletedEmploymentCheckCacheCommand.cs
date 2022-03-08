using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests
{
    public class WhenStoreCompletedEmploymentCheckCacheCommand
    {
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_EmploymentCheck_Is_Retrieved()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Models.EmploymentCheck>();

            // Act
            var cmd = new StoreCompletedEmploymentCheckCommand(employmentCheck);

            // Assert
            Assert.AreEqual(employmentCheck, cmd.EmploymentCheck);
        }
    }
}
