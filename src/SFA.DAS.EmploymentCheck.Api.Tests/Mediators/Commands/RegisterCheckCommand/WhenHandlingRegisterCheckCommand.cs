using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Api.Application.Services;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;

namespace SFA.DAS.EmploymentCheck.Api.Tests.Mediators.Commands.RegisterCheckCommand
{
    public class WhenHandlingRegisterCheckCommand
    {
        private Api.Mediators.Commands.RegisterCheckCommand.RegisterCheckCommand _command;
        private Mock<IEmploymentCheckService> _employmentCheckService;
        private Mock<IRegisterCheckCommandValidator> _commandValidator;

        [SetUp]
        public void Setup()
        {
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _commandValidator = new Mock<IRegisterCheckCommandValidator>();

            _command = new Api.Mediators.Commands.RegisterCheckCommand.RegisterCheckCommand
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

            Assert.AreEqual(result.ErrorMessage, "ErrorMessage");
            Assert.AreEqual(result.ErrorType, "ErrorType");
            Assert.IsNull(result.VersionId);
        }

        [Test]
        public async Task And_The_Command_Is_Valid_And_No_VersionId_Exists_Then_Sets_VersionId_To_One()
        {
            //Arrange

            _commandValidator.Setup(x => x.Validate(_command)).Returns(new RegisterCheckResult());

            _employmentCheckService.Setup(x => x.GetLastEmploymentCheck(_command.CorrelationId))
                .ReturnsAsync((Api.Application.Models.EmploymentCheck) null);

            var sut = new RegisterCheckCommandHandler(_employmentCheckService.Object, _commandValidator.Object);

            //Act

            var result = await sut.Handle(_command, CancellationToken.None);

            //Assert

            Assert.AreEqual(result.VersionId, 1);
            Assert.IsNull(result.ErrorMessage);
            Assert.IsNull(result.ErrorType);
        }

        [Test]
        public async Task And_The_Command_Is_Valid_And_VersionId_Exists_Then_Increments_The_VersionId()
        {
            //Arrange

            _commandValidator.Setup(x => x.Validate(_command)).Returns(new RegisterCheckResult());

            var employmentCheck = new Api.Application.Models.EmploymentCheck
            {
                AccountId = 1, 
                ApprenticeshipId = 2, 
                CheckType = "CheckType", 
                CorrelationId = Guid.NewGuid(),
                CreatedOn = DateTime.Today, 
                Employed = null, 
                Id = 1, 
                LastUpdatedOn = DateTime.Today,
                MinDate = DateTime.Today.AddDays(-1), 
                MaxDate = DateTime.Today.AddDays(1),
                RequestCompletionStatus = null, 
                Uln = 10000001, 
                VersionId = 2
            };
            
            _employmentCheckService.Setup(x => x.GetLastEmploymentCheck(_command.CorrelationId))
                .ReturnsAsync(employmentCheck);

            var sut = new RegisterCheckCommandHandler(_employmentCheckService.Object, _commandValidator.Object);

            //Act

            var result = await sut.Handle(_command, CancellationToken.None);

            //Assert

            Assert.AreEqual(result.VersionId, 3);
            Assert.IsNull(result.ErrorMessage);
            Assert.IsNull(result.ErrorType);
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