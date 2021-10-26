﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.AzureFunctions.Activities.CheckApprenticeEmploymentStatusActivityTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IMediator> _mediator;
        private readonly Mock<ILogger<CheckApprentice>> _logger;

        public WhenCallingGet()
        {
            _mediator = new Mock<IMediator>();

            _logger = new Mock<ILogger<CheckApprentice>>();
        }

        [Fact (Skip = "Method not yet implemented")]
        public void Then_Employment_Check_Results_Are_Returned()
        {
            ////Arrange
            //var sut = new CheckApprenticeEmploymentStatusActivity(_mediator.Object, _logger.Object);

            //_mediator.Setup(x =>
            //        x.Send<CheckApprenticeCommand>(It.IsAny<IRequest<CheckApprenticeCommand>>(),
            //            CancellationToken.None))
            //    .Returns(new CheckApprenticeResponse());

            ////Act
            //var result = sut.Verify(new object()).Result;

            ////Assert
            //Assert.NotNull(result);
            //Assert.Equal(new CheckApprenticeResponse(), result);
        }

        [Fact(Skip = "Method not yet implemented")]
        public void And_Throws_An_Exception_Then_The_Exception_Is_Handled()
        {
            ////Arrange
            //var exception = new Exception();
            //var sut = new CheckApprenticeEmploymentStatusActivity(_mediator.Object, _logger.Object);

            //_mediator.Setup(x => x.Send(It.IsAny<IRequest<CheckApprenticeCommand>>(), CancellationToken.None))
            //    .ThrowsAsync(exception);

            ////Act

            //var result = sut.Verify(new object()).Result;
            //var loggerInvocations = _logger.Invocations.Count;

            ////Assert
            //Assert.Equal(new List<>, result);
            //Assert.Equal(1, loggerInvocations);
        }
    }
}