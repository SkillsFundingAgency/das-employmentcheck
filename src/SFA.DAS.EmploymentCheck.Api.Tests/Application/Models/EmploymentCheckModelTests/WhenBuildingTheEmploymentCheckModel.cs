using System;
using NUnit.Framework;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Application.Models.EmploymentCheckModelTests
{
    public class WhenBuildingTheEmploymentCheckModel
    {
        [Test]
        public void Then_The_Model_Is_Built_Correctly()
        {
            //Arrange

            var expectedGuid = Guid.NewGuid();
            var expectedCheck = "checkType";
            var expectedUln = 1234;
            var expectedApprenticeshipId = 1;
            var expectedAccountId = 2;
            var expectedMinDate = DateTime.Today.AddDays(-1);
            var expectedMaxDate = DateTime.Today.AddDays(1);

            //Act

            var model = new Api.Application.Models.EmploymentCheck(expectedGuid, expectedCheck, expectedUln,
                expectedApprenticeshipId, expectedAccountId, expectedMinDate, expectedMaxDate);

            //Assert

            Assert.AreEqual(expectedGuid, model.CorrelationId);
            Assert.AreEqual(expectedCheck, model.CheckType);
            Assert.AreEqual(expectedUln, model.Uln);
            Assert.AreEqual(expectedApprenticeshipId, model.ApprenticeshipId);
            Assert.AreEqual(expectedAccountId, model.AccountId);
            Assert.AreEqual(expectedMinDate, model.MinDate);
            Assert.AreEqual(expectedMaxDate, model.MaxDate);
        }
    }
}