﻿//using System;
//using System.Collections.Generic;
//using System.Threading;
//using Dynamitey.DynamicObjects;
//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Moq;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
//using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
//using SFA.DAS.EmploymentCheck.Functions.Helpers;
//using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesToVerify;
//using Xunit;

//namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesToVerify.GetApprenticesToVerifyHandlerTests
//{
//    public class WhenHandlingTheRequest
//    {
//        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
//        private readonly Mock<ILogger<GetApprenticesToVerifyHandler>> _logger;

//        public WhenHandlingTheRequest()
//        {
//            _employmentCheckService = new Mock<IEmploymentCheckService>();
//            _logger = new Mock<ILogger<GetApprenticesToVerifyHandler>>();
//        }

//        [Fact]
//        public async void Then_The_EmploymentCheckService_Is_Called()
//        {
//            //Arrange

//            _employmentCheckService.Setup(x => x.GetApprenticeEmploymentChecks()).ReturnsAsync(new List<ApprenticeEmploymentCheck>());

//            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

//            //Act

//            await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

//            //Assert

//            _employmentCheckService.Verify(x => x.GetApprenticeEmploymentChecks(), Times.Exactly(1));
//        }

//        [Fact]
//        public async void And_No_Apprentices_Returned_From_The_EmploymentcheckClient_Then_An_Empty_List_Returned()
//        {
//            //Arrange

//            _employmentCheckService.Setup(x => x.GetApprenticeEmploymentChecks()).ReturnsAsync(new List<ApprenticeEmploymentCheck>());

//            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

//            //Act

//            var result = await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

//            //Assert

//            result.ApprenticesToVerify.Should().BeEquivalentTo(new List<Apprentice>());
//        }

//        [Fact]
//        public async void And_Null_Returned_From_The_EmploymentcheckClient_Then_An_Empty_List_Returned()
//        {
//            //Arrange

//            _employmentCheckService.Setup(x => x.GetApprenticeEmploymentChecks()).ReturnsAsync((List<ApprenticeEmploymentCheck>)null);

//            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

//            //Act

//            var result = await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

//            //Assert

//            result.ApprenticesToVerify.Should().BeEquivalentTo(new List<Apprentice>());
//        }

//        [Fact]
//        public async void And_Apprentices_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Returned()
//        {
//            //Arrange

//            //var apprentice = new ApprenticeEmploymentCheck(
//            //    1,
//            //    1,
//            //    "1000001",
//            //    1000001,
//            //    1000001,
//            //    1,
//            //    DateTime.Today.AddDays(-1),
//            //    DateTime.Today.AddDays(1));
//            //var apprentices = new List<Apprentice> { apprentice };

//            //_employmentCheckService.Setup(x => x.GetApprenticeEmploymentChecks()).ReturnsAsync(apprentices);

//            //var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

//            ////Act

//            //var result = await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

//            ////Assert

//            //Assert.Equal(apprentices, result.ApprenticesToVerify);
//        }

//        [Fact]
//        public async void
//            And_The_EmploymentCheckClient_Throws_An_Exception_Then_An_Empty_List_Is_Returned()
//        {
//            //Arrange

//            var exception = new Exception("Exception");

//            _employmentCheckService.Setup(x => x.GetApprenticeEmploymentChecks()).ThrowsAsync(exception);

//            var sut = new GetApprenticesToVerifyHandler(_employmentCheckService.Object, _logger.Object);

//            //Act

//            var result = await sut.Handle(new GetApprenticesToVerifyRequest(), CancellationToken.None);

//            //Assert

//            result.Should().BeEquivalentTo(new GetApprenticesToVerifyResult(null));
//        }
//    }
//}