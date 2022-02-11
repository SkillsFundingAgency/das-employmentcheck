using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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

        [Test]
        public async Task And_There_Are_No_Errors_Then_An_Empty_Response_And_A_200_Will_Be_Returned()
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
                .ReturnsAsync(new RegisterCheckResult());

            var sut = new EmploymentCheckController(_mediator.Object);

            //Act

            var result = await sut.RegisterCheck(_registerCheckRequest) as OkObjectResult;

            //Assert

            result.Value.Should().BeEquivalentTo(new RegisterCheckResult());
            result.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task And_There_Are_Errors_Then_The_Errors_And_A_400_Will_Be_Returned()
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

            var result = await sut.RegisterCheck(_registerCheckRequest) as BadRequestObjectResult;

            //Assert

            result.Value.Should().BeEquivalentTo(_response);
            result.StatusCode.Should().Be(400);
        }
    }
}