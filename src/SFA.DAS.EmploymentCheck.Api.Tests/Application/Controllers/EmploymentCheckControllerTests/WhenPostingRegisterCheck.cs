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
        private RegisterCheckRequest _registerCheckDto;
        private Guid _correlationId;
        private string _checkType;
        private long _uln;
        private int _apprenticeshipAccountId;
        private long? _apprenticeshipId;
        private DateTime _minDate;
        private DateTime _maxDate;
        private RegisterCheckResult _response;

        [SetUp]
        public void Setup()
        {
            _mediator = new Mock<IMediator>();
            _registerCheckDto = new RegisterCheckRequest
            {
                CorrelationId = new Guid(),
                CheckType = "CheckType",
                Uln = 1000001,
                ApprenticeshipAccountId = 1,
                ApprenticeshipId = 2,
                MinDate = DateTime.Today.AddDays(-1),
                MaxDate = DateTime.Today.AddDays(1),
            };
            _response = new RegisterCheckResult {ErrorMessage = "ErrorMessage", ErrorType = "ErrorType", VersionId = 1};
        }
        [Test]
        public async Task Then_The_Request_Is_Passed_To_Mediator()
        {
            //Arrange

            _mediator.Setup(x => x.Send(It.Is<RegisterCheckCommand>(command =>
                        command.CorrelationId == _registerCheckDto.CorrelationId &&
                        command.CheckType == _registerCheckDto.CheckType &&
                        command.Uln == _registerCheckDto.Uln &&
                        command.ApprenticeshipAccountId == _registerCheckDto.ApprenticeshipAccountId &&
                        command.ApprenticeshipId == _registerCheckDto.ApprenticeshipId &&
                        command.MinDate == _registerCheckDto.MinDate &&
                        command.MaxDate == _registerCheckDto.MaxDate),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_response);

            var sut = new EmploymentCheckController(_mediator.Object);

            //Act

            await sut.RegisterCheck(_registerCheckDto);

            //Assert
            _mediator.Verify(x => x.Send(It.IsAny<RegisterCheckCommand>(), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task And_The_VersionId_Is_Present_Then_It_And_200_Is_Returned()
        {
            //Arrange

            _mediator.Setup(x => x.Send(It.Is<RegisterCheckCommand>(command =>
                        command.CorrelationId == _registerCheckDto.CorrelationId &&
                        command.CheckType == _registerCheckDto.CheckType &&
                        command.Uln == _registerCheckDto.Uln &&
                        command.ApprenticeshipAccountId == _registerCheckDto.ApprenticeshipAccountId &&
                        command.ApprenticeshipId == _registerCheckDto.ApprenticeshipId &&
                        command.MinDate == _registerCheckDto.MinDate &&
                        command.MaxDate == _registerCheckDto.MaxDate),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_response);

            var sut = new EmploymentCheckController(_mediator.Object);

            //Act

            var result = await sut.RegisterCheck(_registerCheckDto) as OkObjectResult;

            var model = result.Value as Responses.RegisterCheckResponse;

            //Assert
            Assert.AreEqual(result.StatusCode.Value, (int)HttpStatusCode.OK);
            Assert.AreEqual(model.VersionId, _response.VersionId);
        }

        [Test]
        public async Task And_The_VersionId_Is_Not_Present_Then_Errors_And_400_Is_Returned()
        {
            //Arrange
            _response.VersionId = null;

            _mediator.Setup(x => x.Send(It.Is<RegisterCheckCommand>(command =>
                        command.CorrelationId == _registerCheckDto.CorrelationId &&
                        command.CheckType == _registerCheckDto.CheckType &&
                        command.Uln == _registerCheckDto.Uln &&
                        command.ApprenticeshipAccountId == _registerCheckDto.ApprenticeshipAccountId &&
                        command.ApprenticeshipId == _registerCheckDto.ApprenticeshipId &&
                        command.MinDate == _registerCheckDto.MinDate &&
                        command.MaxDate == _registerCheckDto.MaxDate),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_response);

            var sut = new EmploymentCheckController(_mediator.Object);

            //Act

            var result = await sut.RegisterCheck(_registerCheckDto) as BadRequestObjectResult;

            var model = result.Value as Responses.RegisterCheckResponse;

            //Assert
            Assert.AreEqual(result.StatusCode.Value, (int)HttpStatusCode.BadRequest);
            Assert.AreEqual(model.ErrorMessage, _response.ErrorMessage);
            Assert.AreEqual(model.ErrorType, _response.ErrorType);
        }
    }
}