using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.StoreEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.StoreEmploymentCheckResult.ResultTests
{
    public class WhenBuildingTheResult
    {
        private Fixture _fixture;
        private long _employmentCheckId;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            // Arrange
            _employmentCheckId = 1;

            //Act
            var result = new StoreEmploymentCheckResultCommandResult(_employmentCheckId);

            //Assert
            Assert.AreEqual(_employmentCheckId, result.EmploymentCheckId);
        }
    }
}