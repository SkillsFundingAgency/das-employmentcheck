using AutoFixture;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Models.Domain.ApprenticeTests
{
    public class WhenBuildingTheModel
    {
        [Fact]
        public void Then_The_Model_Is_Built_Correctly()
        {
            // Arrange
            var fixture = new Fixture();
            var id = fixture.Create<long>();
            var accountId = fixture.Create<long>();
            var nationalInsuranceNumber = fixture.Create<string>();
            var uln = fixture.Create<long>();
            var ukprn = fixture.Create<long>();
            var apprenticeshipId = fixture.Create<long>();
            var startDate = fixture.Create<DateTime>();
            var endDate = fixture.Create<DateTime>();
            var checkType = fixture.Create<string>();
            var isEmployed = fixture.Create<bool>();
            var lastUpdated = fixture.Create<DateTime>();
            var createdDate = fixture.Create<DateTime>();
            var hasBeenChecked= fixture.Create<bool>();

            // Act
            var result = new ApprenticeEmploymentCheckModel(id, uln, nationalInsuranceNumber, ukprn, apprenticeshipId, accountId, startDate, endDate, checkType, isEmployed, lastUpdated, createdDate, hasBeenChecked);

            // Assert
            Assert.Equal(id, result.Id);
            Assert.Equal(uln, result.ULN);
            Assert.Equal(nationalInsuranceNumber, result.NationalInsuranceNumber);
            Assert.Equal(ukprn, result.UKPRN);
            Assert.Equal(apprenticeshipId, result.ApprenticeshipId);
            Assert.Equal(accountId, result.AccountId);
            Assert.Equal(startDate, result.MinDate);
            Assert.Equal(endDate, result.MaxDate);
            Assert.Equal(checkType, result.CheckType);
            Assert.Equal(isEmployed, result.IsEmployed);
            Assert.Equal(lastUpdated, result.LastUpdated);
            Assert.Equal(createdDate, result.CreatedDate);
            Assert.Equal(hasBeenChecked, result.HasBeenChecked);
        }
    }
}