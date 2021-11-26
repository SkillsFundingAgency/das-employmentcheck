//using System;
//using System.Collections.Generic;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
//using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes;
//using Xunit;

//namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetEmployerPayeSchemes.GetEmployersPayeSchemesMediatorRequestTests
//{
//    public class WhenBuildingTheRequest
//    {
//        [Fact]
//        public void Then_The_Request_Is_Built_Successfully()
//        {
//            //Arrange

//            var apprentice = new ApprenticeEmploymentCheck(
//                1,
//                1,
//                "1000001",
//                1000001,
//                1000001,
//                1,
//                DateTime.Today.AddDays(-1),
//                DateTime.Today.AddDays(1));
//            var apprentices = new List<EmploymentCheckModel> {apprentice};

//            //Act

//            var result = new GetEmployersPayeSchemesMediatorRequest(apprentices);

//            //Assert

//            Assert.Equal(apprentices, result.Apprentices);
//        }
//    }
//}