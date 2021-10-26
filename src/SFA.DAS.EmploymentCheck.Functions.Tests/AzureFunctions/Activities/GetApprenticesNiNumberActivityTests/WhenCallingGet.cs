using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetApprenticesNiNumberActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetApprenticesNiNumberActivity>> _logger;
        private readonly Apprentice _apprentice;
        private readonly ApprenticeNiNumber _apprenticeNiNumber;
        private readonly IList<Apprentice> _apprentices;
        public WhenCallingGet()
        {
            _mediator = new Mock<IMediator>();

            _logger = new Mock<ILogger<GetApprenticesNiNumberActivity>>();

            _apprentice = new Apprentice(1,
                1000001,
                "1000001",
                1000001,
                1000001,
                1000001,
                DateTime.Today,
                DateTime.Today.AddDays(1));
            _apprenticeNiNumber = new ApprenticeNiNumber(
                1000001,
                "1000001");

            _apprentices = new List<Apprentice> { _apprentice };
        }

        [Fact]
        public void Then_The_NINumbers_Are_Returned()
        {
            //Arrange
            var sut = new GetApprenticesNiNumberActivity(_mediator.Object, _logger.Object);

            var apprenticeNiNumbers = new GetApprenticesNiNumberMediatorResult(new List<ApprenticeNiNumber> {_apprenticeNiNumber});

            _mediator.Setup(x => x.Send(It.IsAny<GetApprenticesNiNumberMediatorRequest>(), CancellationToken.None))
                .ReturnsAsync(apprenticeNiNumbers);
            
            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(apprenticeNiNumbers.ApprenticesNiNumber.Count, result.Count);
            Assert.Equal(apprenticeNiNumbers.ApprenticesNiNumber, result);
        }
        [Fact]
        public void And_Throws_An_Expection_Then_Exception_Is_Handled()
        {
            //Arrange
            var exception = new Exception();
            var sut = new GetApprenticesNiNumberActivity(_mediator.Object, _logger.Object);

            _mediator.Setup(x => x.Send(It.IsAny<GetApprenticesNiNumberMediatorRequest>(), CancellationToken.None))
                .ThrowsAsync(exception);

            //Act

            var result = sut.Get(_apprentices).Result;
            var loggerInvocations = _logger.Invocations.Count;

            //Assert
            Assert.Equal(new List<ApprenticeNiNumber>(), result);
            Assert.Equal(1, loggerInvocations);

        }
    }
}