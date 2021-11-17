//using System;
//using System.Collections.Generic;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
//using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks;
//using Xunit;

//namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprentices.GetApprenticesMediatorResultTests
//{
//    public class WhenBuildingTheResult
//    {

//        [Fact]
//        public void Then_The_Result_Is_Built_Correctly()
//        {
//            //Arrange
//            var apprentice = new Apprentice(1,
//                1,
//                "1000001",
//                1000001,
//                1000001,
//                1,
//                DateTime.Today.AddDays(-1),
//                DateTime.Today.AddDays(1)
//                );
//            var apprentices = new List<Apprentice> {apprentice};

//            //Act

//            var result = new GetApprenticeEmploymentChecksQueryResult(apprentices);

//            //Assert

//            Assert.Equal(apprentices, result.Apprentices);
//        }
//    }
//}