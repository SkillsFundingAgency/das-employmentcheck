﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Application.Controllers;
using SFA.DAS.EmploymentCheck.Commands.RegisterCheck;

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
        public async Task And_The_Command_Is_Accepted_Then_200_Is_Returned()
        {
            //Arrange

            _response.ErrorMessage = null;
            _response.ErrorType = null;

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

            var result = await sut.RegisterCheck(_registerCheckRequest) as OkObjectResult;

            var model = result?.Value as Responses.RegisterCheckResponse;

            //Assert
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual(result?.StatusCode.Value, (int)HttpStatusCode.OK);
        }

        [Test]
        public async Task And_The_Command_Is_Not_Accepted_Then_Errors_And_400_Is_Returned()
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

            var model = result?.Value as Responses.RegisterCheckResponse;

            //Assert
            // ReSharper disable once PossibleInvalidOperationException
            Assert.AreEqual(result?.StatusCode.Value, (int)HttpStatusCode.BadRequest);
            Assert.AreEqual(model?.ErrorMessage, _response.ErrorMessage);
            Assert.AreEqual(model?.ErrorType, _response.ErrorType);
        }
    }
}