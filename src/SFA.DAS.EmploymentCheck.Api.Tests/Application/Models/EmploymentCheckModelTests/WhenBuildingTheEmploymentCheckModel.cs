using System;
using AutoFixture;
using NUnit.Framework;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Application.Models.EmploymentCheckModelTests
{
    public class WhenBuildingTheEmploymentCheckModel
    {
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new Fixture();
        }
        [Test]
        public void Then_The_Model_Is_Built_Correctly()
        {
            //Arrange

            var expectedGuid = _fixture.Create<Guid>();
            var expectedCheck = _fixture.Create<string>();
            var expectedUln = _fixture.Create<long>();
            var expectedApprenticeshipId = _fixture.Create<long>();
            var expectedAccountId = _fixture.Create<long>();
            var expectedMinDate = _fixture.Create<DateTime>();
            var expectedMaxDate = _fixture.Create<DateTime>();

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