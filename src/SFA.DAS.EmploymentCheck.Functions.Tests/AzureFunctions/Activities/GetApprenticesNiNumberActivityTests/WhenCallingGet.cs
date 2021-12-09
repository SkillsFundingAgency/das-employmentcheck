using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetApprenticesNiNumberActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetApprenticesNiNumberActivity>> _logger;
        private readonly ApprenticeNiNumber _apprenticeNiNumber;
        private readonly IList<ApprenticeEmploymentCheckModel> _apprentices;

        public WhenCallingGet()
        {
            var fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetApprenticesNiNumberActivity>>();
            _apprenticeNiNumber = fixture.Create<ApprenticeNiNumber>();
            _apprentices = new List<ApprenticeEmploymentCheckModel> { fixture.Create<ApprenticeEmploymentCheckModel>() };
        }

        [Fact]
        public void Then_The_NINumbers_Are_Returned()
        {
            //Arrange
            var sut = new GetApprenticesNiNumberActivity(_mediator.Object, _logger.Object);

            var apprenticeNiNumbers = new GetApprenticesNiNumberMediatorResult(new List<ApprenticeNiNumber> { _apprenticeNiNumber });

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
        public void And_Throws_An_Exception_Then_Exception_Is_Handled()
        {
            //Arrange
            var exception = new Exception("test message");
            var sut = new GetApprenticesNiNumberActivity(_mediator.Object, _logger.Object);

            _mediator.Setup(x => x.Send(It.IsAny<GetApprenticesNiNumberMediatorRequest>(), CancellationToken.None))
                .ThrowsAsync(exception);

            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.Equal(new List<ApprenticeNiNumber>(), result);
        }
    }
}