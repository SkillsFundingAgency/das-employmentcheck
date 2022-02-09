using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Enums;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus;
using System;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.SetCacheRequestRelatedRequestsProcessingStatus.CommandTests
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
            var employmentCheckCacheRequestAndStatusToSet = _fixture.Create<Tuple<EmploymentCheckCacheRequest, ProcessingCompletionStatus>>();

            //Act
            var command = new SetCacheRequestRelatedRequestsProcessingStatusCommand(employmentCheckCacheRequestAndStatusToSet);

            //Assert
            Assert.AreEqual(command.EmploymentCheckCacheRequestAndStatusToSet, employmentCheckCacheRequestAndStatusToSet);
        }
    }
}