using AutoFixture;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.SetEmploymentCheckCacheRequestRelatedRequestsRequestProcessingStatus;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Commands.SetCacheRequestRelatedRequestsProcessingStatus.ResultTests
{
    public class WhenBuildingTheResult
    {
        private Fixture _fixture;
        private IList<EmploymentCheckCacheRequest> _employmentCheckCacheRequests;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Then_The_Result_Is_Built_Successfully()
        {
            // Arrange
            _employmentCheckCacheRequests = new List<EmploymentCheckCacheRequest> { _fixture.Create<EmploymentCheckCacheRequest>() };

            //Act
            var result = new SetCacheRequestRelatedRequestsProcessingStatusCommandResult(_employmentCheckCacheRequests);

            //Assert
            Assert.AreEqual(_employmentCheckCacheRequests, result.EmploymentCheckCacheRequests);
        }
    }
}