//using System;
//using System.Collections.Generic;
//using Dynamitey.DynamicObjects;
//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Moq;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
//using SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData;
//using SFA.DAS.EmploymentCheck.Functions.Helpers;
//using Xunit;

//namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Clients.SubmitLearnerData.SubmitLearnerDataClientTests
//{
//    public class WhenGettingApprenticesNiNumber
//    {
//        private readonly Mock<ISubmitLearnerDataService> _submitLearnerDataService;
//        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;

//        public WhenGettingApprenticesNiNumber()
//        {
//            _submitLearnerDataService = new Mock<ISubmitLearnerDataService>();
//            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
//        }

//        [Fact]
//        public async void Then_The_SubmitLeanerDataService_Is_Called()
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
//            var apprentices = new List<ApprenticeEmploymentCheck> {apprentice};

//            _submitLearnerDataService.Setup(x => x.GetApprenticesNiNumber(apprentices))
//                .ReturnsAsync(new List<ApprenticeNiNumber>());

//            var sut = new SubmitLearnerDataClient(_submitLearnerDataService.Object, _logger.Object);

//            //Act

//            await sut.GetApprenticesNiNumber(apprentices);

//            //Assert

//            _submitLearnerDataService.Verify(x => x.GetApprenticesNiNumber(apprentices), Times.Exactly(1));
//        }

//        [Fact]
//        public async void
//            And_The_SubmitLearnerDataService_Returns_No_Ni_Numbers_Then_An_Empty_List_Is_Returned()
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
//            var apprentices = new List<ApprenticeEmploymentCheck> { apprentice };

//            _submitLearnerDataService.Setup(x => x.GetApprenticesNiNumber(apprentices))
//                .ReturnsAsync(new List<ApprenticeNiNumber>());

//            var sut = new SubmitLearnerDataClient(_submitLearnerDataService.Object, _logger.Object);

//            //Act

//            var result = await sut.GetApprenticesNiNumber(apprentices);

//            //Assert

//            result.Should().BeEquivalentTo(new List<ApprenticeNiNumber>());
//        }
//        [Fact]
//        public async void
//            And_The_SubmitLearnerDataService_Returns_Null_Then_An_Empty_List_Is_Returned()
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
//            var apprentices = new List<ApprenticeEmploymentCheck> { apprentice };

//            _submitLearnerDataService.Setup(x => x.GetApprenticesNiNumber(apprentices))
//                .ReturnsAsync((List<ApprenticeNiNumber>)null);

//            var sut = new SubmitLearnerDataClient(_submitLearnerDataService.Object, _logger.Object);

//            //Act

//            var result = await sut.GetApprenticesNiNumber(apprentices);

//            //Assert

//            result.Should().BeEquivalentTo(new List<ApprenticeNiNumber>());
//        }

//        [Fact]
//        public async void
//            And_The_SubmitLearnerDataService_Returns_Ni_Numbers_Then_They_Are_Returned()
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
//            var apprentices = new List<ApprenticeEmploymentCheck> { apprentice };

//            var niNumber = new ApprenticeNiNumber(1000001, "1000001");
//            var niNumbers = new List<ApprenticeNiNumber> {niNumber};

//            _submitLearnerDataService.Setup(x => x.GetApprenticesNiNumber(apprentices))
//                .ReturnsAsync(niNumbers);

//            var sut = new SubmitLearnerDataClient(_submitLearnerDataService.Object, _logger.Object);

//            //Act

//            var result = await sut.GetApprenticesNiNumber(apprentices);

//            //Assert

//            Assert.Equal(niNumbers, result);
//        }

//        [Fact]
//        public async void And_No_Apprentices_Are_Passed_In_Then_An_Empty_List_Is_Returned()
//        {
//            //Arrange

//            var sut = new SubmitLearnerDataClient(_submitLearnerDataService.Object, _logger.Object);

//            //Act

//            var result = await sut.GetApprenticesNiNumber(null);

//            //Assert

//            result.Should().BeEquivalentTo(new List<ApprenticeNiNumber>());
//        }

//        [Fact]
//        public async void And_An_Exception_Is_Thrown_Then_An_Empty_List_Is_Returned()
//        {
//            //Arrange

//            var exception = new Exception("exception");
//            _submitLearnerDataService.Setup(x => x.GetApprenticesNiNumber(new List<ApprenticeEmploymentCheck>()))
//                .ThrowsAsync(exception);

//            var sut = new SubmitLearnerDataClient(_submitLearnerDataService.Object, _logger.Object);

//            //Act

//            var result = await sut.GetApprenticesNiNumber(new List<ApprenticeEmploymentCheck>());

//            //Assert

//            result.Should().BeEquivalentTo(new List<ApprenticeNiNumber>());
//        }
//    }
//}