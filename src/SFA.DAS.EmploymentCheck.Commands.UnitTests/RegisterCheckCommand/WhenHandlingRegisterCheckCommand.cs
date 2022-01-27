using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Commands.RegisterCheck;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.RegisterCheckCommand
{
    public class WhenHandlingRegisterCheckCommand
    {
        private RegisterCheck.RegisterCheckCommand _command;
        private Mock<IEmploymentCheckService> _employmentCheckService;
        private Mock<IRegisterCheckCommandValidator> _commandValidator;

        [SetUp]
        public void Setup()
        {
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _commandValidator = new Mock<IRegisterCheckCommandValidator>();

            _command = new RegisterCheck.RegisterCheckCommand
            {
                ApprenticeshipAccountId = 1,
                ApprenticeshipId = 2,
                CheckType = "CheckType",
                CorrelationId = Guid.NewGuid(),
                MaxDate = DateTime.Today.AddDays(1),
                MinDate = DateTime.Today.AddDays(-1),
                Uln = 1000001
            };
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
        public async Task And_The_Command_Is_Valid_Then_Inserts_The_Row_And_Returns_No_Errors()
        {
            //Arrange

            _commandValidator.Setup(x => x.Validate(_command)).Returns(new RegisterCheckResult());

            _employmentCheckService.Setup(x => x.GetLastEmploymentCheck(_command.CorrelationId))
                .ReturnsAsync((Data.Models.EmploymentCheck)null);

            var sut = new RegisterCheckCommandHandler(_employmentCheckService.Object, _commandValidator.Object);

            //Act

            var result = await sut.Handle(_command, CancellationToken.None);

            //Assert

            _employmentCheckService.Verify(x => x.InsertEmploymentCheck(It.IsAny<Data.Models.EmploymentCheck>()), Times.Once);
            Assert.IsNull(result.ErrorMessage);
            Assert.IsNull(result.ErrorType);
        }
    }
}