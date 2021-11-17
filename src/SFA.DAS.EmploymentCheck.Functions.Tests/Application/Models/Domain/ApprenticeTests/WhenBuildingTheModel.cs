//using System;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
//using Xunit;

//namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Models.Domain.ApprenticeTests
//{
//    public class WhenBuildingTheModel
//    {
//        [Fact]
//        public void Then_The_Model_Is_Built_Correctly()
//        {
//            //Arrange

//            var id = 1;
//            var accountId = 1;
//            var nationalInsuranceNumber = "nationalInsuranceNumber";
//            var uln = 1000001;
//            var ukprn = 1000001;
//            var apprenticeshipId = 1;
//            var startDate = DateTime.Today.AddDays(-1);
//            var endDate = DateTime.Today.AddDays(1);

//            //Act

//            var result = new ApprenticeEmploymentCheck(id, accountId, nationalInsuranceNumber, uln, ukprn, apprenticeshipId, startDate, endDate);

//            //Assert

//            Assert.Equal(id, result.Id);
//            Assert.Equal(accountId, result.AccountId);
//            Assert.Equal(nationalInsuranceNumber, result.NationalInsuranceNumber);
//            Assert.Equal(uln, result.ULN);
//            Assert.Equal(ukprn, result.UKPRN);
//            Assert.Equal(apprenticeshipId, result.ApprenticeshipId);
//            Assert.Equal(startDate, result.StartDate);
//            Assert.Equal(endDate, result.EndDate);
//        }
//    }
//}