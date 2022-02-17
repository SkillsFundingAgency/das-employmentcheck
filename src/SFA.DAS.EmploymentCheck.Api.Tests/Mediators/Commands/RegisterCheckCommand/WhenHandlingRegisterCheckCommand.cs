using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Application.Services;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Mediators.Commands.RegisterCheckCommand
{
    public class WhenHandlingRegisterCheckCommand
    {
        private Fixture _fixture;
        private Api.Mediators.Commands.RegisterCheckCommand.RegisterCheckCommand _command;
        private Mock<IEmploymentCheckService> _employmentCheckService;
        private Mock<IRegisterCheckCommandValidator> _commandValidator;

        [SetUp]
        public void Setup()
        {
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _commandValidator = new Mock<IRegisterCheckCommandValidator>();

            _fixture = new Fixture();

            _command = _fixture.Create<Api.Mediators.Commands.RegisterCheckCommand.RegisterCheckCommand>();
        }

        [Test]
        public async Task And_The_Command_Is_Invalid_Then_Returns_Result_With_Errors()
        {
            //Arrange

            _commandValidator.Setup(x => x.Validate(_command)).
                Returns(new RegisterCheckResult {ErrorMessage = "ErrorMessage", ErrorType = "ErrorType"});

            var sut = new RegisterCheckCommandHandler(_employmentCheckService.Object, _commandValidator.Object);

            //Act

            var result = await sut.Handle(_command, CancellationToken.None);

            //Assert

            Assert.AreEqual("ErrorMessage", result.ErrorMessage);
            Assert.AreEqual("ErrorType", result.ErrorType);
        }

        [Test]
        public async Task And_The_Command_Is_Valid_Then_Inserts_The_Row()
        {
            //Arrange

            _commandValidator.Setup(x => x.Validate(_command)).Returns(new RegisterCheckResult());

            _employmentCheckService.Setup(x => x.GetLastEmploymentCheck(_command.CorrelationId))
                .ReturnsAsync((Api.Application.Models.EmploymentCheck)null);

            var sut = new RegisterCheckCommandHandler(_employmentCheckService.Object, _commandValidator.Object);

            //Act

            await sut.Handle(_command, CancellationToken.None);

            //Assert

            _employmentCheckService.Verify(x => x.InsertEmploymentCheck(It.IsAny<Api.Application.Models.EmploymentCheck>()), Times.Once);
        }
    }
}