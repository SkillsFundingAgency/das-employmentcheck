using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticeEmploymentChecks;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.GetApprenticesActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<GetApprenticeEmploymentChecksActivity>> _logger;
        private readonly Fixture _fixture;

        public WhenCallingGet()
        {
            _fixture = new Fixture();
            _mediator = new Mock<IMediator>();
            _logger = new Mock<ILogger<GetApprenticeEmploymentChecksActivity>>();
        }

        [Fact]
        public void Then_Apprentices_Are_Returned()
        {
            //Arrange
            var apprentices = new List<ApprenticeEmploymentCheckModel> { _fixture.Create<ApprenticeEmploymentCheckModel>() };
            var sut = new GetApprenticeEmploymentChecksActivity(_mediator.Object, _logger.Object);

            _mediator.Setup(x => x.Send(It.IsAny<GetApprenticeEmploymentChecksQueryRequest>(), CancellationToken.None))
                .ReturnsAsync(new GetApprenticeEmploymentChecksQueryResult(apprentices));

            //Act

            var result = sut.Get(_fixture.Create<long>()).Result;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(apprentices.Count, result.Count);
            Assert.Equal(apprentices, result);
        }

        [Fact]
        public void And_Throws_An_Exception_Then_Exception_Is_Handled()
        {
            //Arrange
            var exception = new Exception("test message");
            var sut = new GetApprenticeEmploymentChecksActivity(_mediator.Object, _logger.Object);

            _mediator.Setup(x => x.Send(It.IsAny<GetApprenticeEmploymentChecksQueryRequest>(), CancellationToken.None))
                .ThrowsAsync(exception);

            //Act

            var result = sut.Get(_fixture.Create<long>()).Result;

            //Assert
            Assert.Equal(new List<ApprenticeEmploymentCheckModel>(), result);

        }
    }
}