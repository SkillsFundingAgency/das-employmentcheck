//using System;
//using System.Collections.Generic;
//using System.Threading;
//using Castle.Core.Logging;
//using Dynamitey.DynamicObjects;
//using MediatR;
//using Microsoft.Extensions.Logging;
//using Moq;
//using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
//using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
//using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks;
//using Xunit;

//namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetApprenticesActivityTests
//{
//    public class WhenCallingGet
//    {
//        private readonly Mock<IMediator> _mediator;
//        private readonly Mock<ILogger<GetApprenticeEmploymentChecksActivity>> _logger;
//        private readonly ApprenticeEmploymentCheck _apprentice;
//        public WhenCallingGet()
//        {
//            _mediator = new Mock<IMediator>();
//            _logger = new Mock<ILogger<GetApprenticeEmploymentChecksActivity>>();
//            _apprentice = new ApprenticeEmploymentCheck(1,
//                1000001,
//                "1000001",
//                1000001,
//                1000001,
//                1000001,
//                DateTime.Today,
//                DateTime.Today.AddDays(1));
//        }

//        [Fact]
//        public void Then_Apprentices_Are_Returned()
//        {
//            //Arrange
//            var apprentices = new List<ApprenticeEmploymentCheck>{_apprentice};
//            var sut = new GetApprenticeEmploymentChecksActivity(_mediator.Object, _logger.Object);

//            _mediator.Setup(x => x.Send(It.IsAny<GetApprenticeEmploymentChecksQueryRequest>(), CancellationToken.None))
//                .ReturnsAsync(new GetApprenticeEmploymentChecksQueryResult(apprentices));

//            //Act

//            var result = sut.Get(new object()).Result;

//            //Assert
//            Assert.NotNull(result);
//            Assert.Equal(apprentices.Count, result.Count);
//            Assert.Equal(apprentices, result);
//        }

//        [Fact]
//        public void And_Throws_An_Expection_Then_Exception_Is_Handled()
//        {
//            //Arrange
//            var exception = new Exception("test message");
//            var sut = new GetApprenticeEmploymentChecksActivity(_mediator.Object, _logger.Object);

//            _mediator.Setup(x => x.Send(It.IsAny<GetApprenticeEmploymentChecksQueryRequest>(), CancellationToken.None))
//                .ThrowsAsync(exception);

//            //Act

//            var result =  sut.Get(new object()).Result;

//            //Assert
//            Assert.Equal(new List<Apprentice>(), result);

//        }
//    }
//}