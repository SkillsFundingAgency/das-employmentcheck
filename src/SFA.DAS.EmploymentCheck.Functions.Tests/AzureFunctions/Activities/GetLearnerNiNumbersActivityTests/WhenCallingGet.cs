using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetLearnerNiNumbersActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetLearnerNiNumbersActivity>> _logger;
        private readonly LearnerNiNumber _apprenticeNiNumber;
        private readonly IList<Functions.Application.Models.EmploymentCheck> _apprentices;

        public WhenCallingGet()
        {
            var fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetLearnerNiNumbersActivity>>();
            _apprenticeNiNumber = fixture.Create<LearnerNiNumber>();
            _apprentices = new List<Functions.Application.Models.EmploymentCheck> { fixture.Create<Functions.Application.Models.EmploymentCheck>() };
        }

        [Fact]
        public void Then_The_NINumbers_Are_Returned()
        {
            //Arrange
            var sut = new GetLearnerNiNumbersActivity(_mediator.Object, _logger.Object);

            var apprenticeNiNumbers = new GetNiNumbersQueryResult(new List<LearnerNiNumber> { _apprenticeNiNumber });

            _mediator.Setup(x => x.Send(It.IsAny<GetNiNumbersQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(apprenticeNiNumbers);

            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(apprenticeNiNumbers.LearnerNiNumber.Count, result.Count);
            Assert.Equal(apprenticeNiNumbers.LearnerNiNumber, result);
        }
        [Fact]
        public void And_Throws_An_Exception_Then_Exception_Is_Handled()
        {
            //Arrange
            var exception = new Exception("test message");
            var sut = new GetLearnerNiNumbersActivity(_mediator.Object, _logger.Object);

            _mediator.Setup(x => x.Send(It.IsAny<GetNiNumbersQueryRequest>(), CancellationToken.None))
                .ThrowsAsync(exception);

            //Act
            var result = sut.Get(_apprentices).Result;

            //Assert
            Assert.Equal(new List<LearnerNiNumber>(), result);
        }
    }
}