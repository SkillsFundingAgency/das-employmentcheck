using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Application.Controllers;
using SFA.DAS.EmploymentCheck.Api.Application.Models;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Application.Controllers.EmploymentCheckControllerTests
{
    public class WhenPostingRegisterCheck
    {
        private Mock<IMediator> _mediator;
        private RegisterCheckRequest _registerCheckRequest;
        private RegisterCheckResult _response;

        [SetUp]
        public void Setup()
        {
            _mediator = new Mock<IMediator>();
            _registerCheckRequest = new RegisterCheckRequest
            {
                CorrelationId = Guid.NewGuid(),
                CheckType = "CheckType",
                Uln = 1000001,
                ApprenticeshipAccountId = 1,
                ApprenticeshipId = 2,
                MinDate = DateTime.Today.AddDays(-1),
                MaxDate = DateTime.Today.AddDays(1),
            };
            _response = new RegisterCheckResult {ErrorMessage = "ErrorMessage", ErrorType = "ErrorType"};
        }
        [Test]
        public async Task Then_The_Request_Is_Passed_To_Mediator()
        {
            //Arrange

            _mediator.Setup(x => x.Send(It.Is<RegisterCheckCommand>(command =>
                        command.CorrelationId == _registerCheckRequest.CorrelationId &&
                        command.CheckType == _registerCheckRequest.CheckType &&
                        command.Uln == _registerCheckRequest.Uln &&
                        command.ApprenticeshipAccountId == _registerCheckRequest.ApprenticeshipAccountId &&
                        command.ApprenticeshipId == _registerCheckRequest.ApprenticeshipId &&
                        command.MinDate == _registerCheckRequest.MinDate &&
                        command.MaxDate == _registerCheckRequest.MaxDate),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_response);

            var sut = new EmploymentCheckController(_mediator.Object);

            //Act

            await sut.RegisterCheck(_registerCheckRequest);

            //Assert
            _mediator.Verify(x => x.Send(It.IsAny<RegisterCheckCommand>(), CancellationToken.None), Times.Once);
        }
    }
}