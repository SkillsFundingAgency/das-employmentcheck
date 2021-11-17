//using System;
//using System.Collections.Generic;
//using System.Threading;
//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Moq;
//using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
//using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks;
//using Xunit;

//namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprentices.GetApprenticesMediatorHandlerTests
//{
//    public class WhenHandlingTheRequest
//    {
//        private readonly Mock<IEmploymentCheckClient> _employmentCheckClient;
//        private readonly Mock<ILogger<GetApprenticeEmploymentChecksQueryHandler>> _logger;

//        public WhenHandlingTheRequest()
//        {
//            _employmentCheckClient = new Mock<IEmploymentCheckClient>();
//            _logger = new Mock<ILogger<GetApprenticeEmploymentChecksQueryHandler>>();
//        }

//        [Fact]
//        public async void Then_The_EmploymentCheckClient_Is_Called()
//        {
//            //Arrange

//            _employmentCheckClient.Setup(x => x.GetApprenticeEmploymentChecks()).ReturnsAsync(new List<Apprentice>());

//            var sut = new GetApprenticeEmploymentChecksQueryHandler(_employmentCheckClient.Object, _logger.Object);

//            //Act

//            await sut.Handle(new GetApprenticeEmploymentChecksQueryRequest(), CancellationToken.None);

//            //Assert

//            _employmentCheckClient.Verify(x => x.GetApprenticeEmploymentChecks(), Times.Exactly(1));
//        }

//        [Fact]
//        public async void And_No_Apprentices_Returned_From_The_EmploymentcheckClient_Then_An_Empty_List_Returned()
//        {
//            //Arrange

//            _employmentCheckClient.Setup(x => x.GetApprenticeEmploymentChecks()).ReturnsAsync(new List<Apprentice>());

//            var sut = new GetApprenticeEmploymentChecksQueryHandler(_employmentCheckClient.Object, _logger.Object);

//            //Act

//            var result = await sut.Handle(new GetApprenticeEmploymentChecksQueryRequest(), CancellationToken.None);

//            //Assert

//            result.Apprentices.Should().BeEquivalentTo(new List<Apprentice>());
//        }

//        [Fact]
//        public async void And_Null_Returned_From_The_EmploymentcheckClient_Then_An_Empty_List_Returned()
//        {
//            //Arrange

//            _employmentCheckClient.Setup(x => x.GetApprenticeEmploymentChecks()).ReturnsAsync((List<Apprentice>)null);

//            var sut = new GetApprenticeEmploymentChecksQueryHandler(_employmentCheckClient.Object, _logger.Object);

//            //Act

//            var result = await sut.Handle(new GetApprenticeEmploymentChecksQueryRequest(), CancellationToken.None);

//            //Assert

//            result.Apprentices.Should().BeEquivalentTo(new List<Apprentice>());
//        }

//        [Fact]
//        public async void And_Apprentices_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Returned()
//        {
//            //Arrange

//            var apprentice = new Apprentice(1,
//                1,
//                "1000001",
//                1000001,
//                1000001,
//                1,
//                DateTime.Today.AddDays(-1),
//                DateTime.Today.AddDays(1));
//            var apprentices = new List<Apprentice> { apprentice };

//            _employmentCheckClient.Setup(x => x.GetApprenticeEmploymentChecks()).ReturnsAsync(apprentices);

//            var expected = new GetApprenticeEmploymentChecksQueryResult(apprentices);

//            var sut = new GetApprenticeEmploymentChecksQueryHandler(_employmentCheckClient.Object, _logger.Object);

//            //Act

//            var result = await sut.Handle(new GetApprenticeEmploymentChecksQueryRequest(), CancellationToken.None);

//            //Assert

//            result.Should().BeEquivalentTo(expected);
//        }

//        [Fact]
//        public async void
//            And_The_EmploymentCheckClient_Throws_An_Exception_Then_An_Empty_List_Is_Returned()
//        {
//            //Arrange

//            var exception = new Exception("Exception");

//            _employmentCheckClient.Setup(x => x.GetApprenticeEmploymentChecks()).ThrowsAsync(exception);

//            var sut = new GetApprenticeEmploymentChecksQueryHandler(_employmentCheckClient.Object, _logger.Object);

//            //Act

//            var result = await sut.Handle(new GetApprenticeEmploymentChecksQueryRequest(), CancellationToken.None);

//            //Assert

//            result.Should().BeEquivalentTo(new GetApprenticeEmploymentChecksQueryResult(null));
//        }
//    }
//}