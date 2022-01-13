using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Controllers;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Controllers.EmploymentCheckControllerTests
{
    public class WhenPostingRegisterCheck
    {
        private Mock<IMediator> _mediator;
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

            _correlationId = new Guid();
            _checkType = "CheckType";
            _uln = 1000001;
            _apprenticeshipAccountId = 1;
            _apprenticeshipId = 2;
            _minDate = DateTime.Today.AddDays(-1);
            _maxDate = DateTime.Today.AddDays(1);

            _response = new RegisterCheckResult
                {
                ErrorMessage = "ErrorMessage", ErrorType = "ErrorType", VersionId = 1};
        }
        [Test]
        public async Task Then_The_Request_Is_Passed_To_Mediator()
        {
            //Arrange
            
            _mediator.Setup(x => x.Send(It.Is<RegisterCheckCommand>(command => 
                    command.CorrelationId == _correlationId &&
                    command.CheckType == _checkType &&
                    command.Uln == _uln &&
                    command.ApprenticeshipAccountId == _apprenticeshipAccountId &&
                    command.ApprenticeshipId == _apprenticeshipId &&
                    command.MinDate == _minDate &&
                    command.MaxDate == _maxDate), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_response);

            var sut = new EmploymentCheckController(_mediator.Object);

            //Act

            await sut.RegisterCheck(_correlationId, _checkType, _uln, _apprenticeshipAccountId, _apprenticeshipId, _minDate,
                _maxDate);

            //Assert
            _mediator.Verify(x => x.Send(It.IsAny<RegisterCheckCommand>(), CancellationToken.None), Times.Once);
        }

        [Test]
        public async Task And_The_VersionId_Is_Present_Then_It_And_200_Is_Returned()
        {
            //Arrange

            _mediator.Setup(x => x.Send(It.Is<RegisterCheckCommand>(command =>
                        command.CorrelationId == _correlationId &&
                        command.CheckType == _checkType &&
                        command.Uln == _uln &&
                        command.ApprenticeshipAccountId == _apprenticeshipAccountId &&
                        command.ApprenticeshipId == _apprenticeshipId &&
                        command.MinDate == _minDate &&
                        command.MaxDate == _maxDate),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_response);

            var sut = new EmploymentCheckController(_mediator.Object);

            //Act

            var result = await sut.RegisterCheck(_correlationId, _checkType, _uln, _apprenticeshipAccountId, _apprenticeshipId, _minDate,
                _maxDate) as OkObjectResult;

            var model = result.Value as Responses.RegisterCheckResponse;

            //Assert
            Assert.AreEqual(result.StatusCode.Value, (int)HttpStatusCode.OK);
            Assert.AreEqual(model.VersionId, _response.VersionId);
        }

        [Test]
        public async Task And_The_VersionId_Is_Not_Present_Then_Errors_And_400_Is_Returned()
        {
            //Arrange
            _response.VersionId = 0;

            _mediator.Setup(x => x.Send(It.Is<RegisterCheckCommand>(command =>
                        command.CorrelationId == _correlationId &&
                        command.CheckType == _checkType &&
                        command.Uln == _uln &&
                        command.ApprenticeshipAccountId == _apprenticeshipAccountId &&
                        command.ApprenticeshipId == _apprenticeshipId &&
                        command.MinDate == _minDate &&
                        command.MaxDate == _maxDate),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_response);

            var sut = new EmploymentCheckController(_mediator.Object);

            //Act

            var result = await sut.RegisterCheck(_correlationId, _checkType, _uln, _apprenticeshipAccountId, _apprenticeshipId, _minDate,
                _maxDate) as BadRequestObjectResult;

            var model = result.Value as Responses.RegisterCheckResponse;

            //Assert
            Assert.AreEqual(result.StatusCode.Value, (int)HttpStatusCode.BadRequest);
            Assert.AreEqual(model.ErrorMessage, _response.ErrorMessage);
            Assert.AreEqual(model.ErrorType, _response.ErrorType);
        }
    }
}