using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Commands.CheckApprentice.CheckApprenticeCommandHandlerTests
{
    public class WhenHandlingTheCommand
    {
        private readonly Mock<ILogger<CheckApprenticeCommandHandler>> _logger;
        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
        private readonly Mock<IEmployerAccountService> _accountsService;
        private readonly Mock<IHmrcService> _hmrcService;
        private readonly Apprentice _apprentice;

        public WhenHandlingTheCommand()
        {
            _logger = new Mock<ILogger<CheckApprenticeCommandHandler>>();
            _accountsService = new Mock<IEmployerAccountService>();
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _hmrcService = new Mock<IHmrcService>();

            _apprentice = new Apprentice(1,
                1,
                "1000001",
                1000001,
                1000001,
                1000001,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
        }

        [Fact]
        public async void And_No_Paye_Schemes_Returned_Then_None_Returned_Is_Logged()
        {
            //Arrange
            
            var command = new CheckApprenticeCommand(_apprentice);

            var sut = new CheckApprenticeCommandHandler(_employmentCheckService.Object, _accountsService.Object,
                _hmrcService.Object, _logger.Object);

            _accountsService.Setup(x => x.GetEmployerAccount(command.Apprentice.AccountId))
                .ReturnsAsync(new AccountDetailViewModel{PayeSchemes = new ResourceList(new List<ResourceViewModel>())});

            //Act

            await sut.Handle(command, CancellationToken.None);

            //Assert

            _logger.Verify(x =>
                x.LogInformation(
                    $"{DateTime.UtcNow} CheckApprenticeCommandHandler.Handle(): GetAccountPayeSchemes() returned null/zero PAYE schemes."));
        }

        [Fact]
        public async void And_Throws_An_Exception_Then_It_Is_Logged()
        {
            //Arrange

            var command = new CheckApprenticeCommand(_apprentice);

            var sut = new CheckApprenticeCommandHandler(_employmentCheckService.Object, _accountsService.Object,
                _hmrcService.Object, _logger.Object);

            var exception = new Exception("test");

            _accountsService.Setup(x => x.GetEmployerAccount(command.Apprentice.AccountId))
                .Throws(exception);

            //Act

            await sut.Handle(command, CancellationToken.None);

            //Assert
            _logger.Verify(x => x.LogInformation($"Exception caught - {exception.Message}. {exception.StackTrace}"));
        }

        [Fact]
        public async void Then_The_HmrcService_Is_Called()
        {
            //Arrange

            var command = new CheckApprenticeCommand(_apprentice);

            var sut = new CheckApprenticeCommandHandler(_employmentCheckService.Object, _accountsService.Object,
                _hmrcService.Object, _logger.Object);

            var paye = new ResourceViewModel();
            paye.Id = "testId";
            var payes = new ResourceList(new List<ResourceViewModel> {paye});

            _accountsService.Setup(x => x.GetEmployerAccount(command.Apprentice.AccountId))
                .ReturnsAsync(new AccountDetailViewModel { PayeSchemes = payes});

            //Act

            await sut.Handle(command, CancellationToken.None);

            //Assert

            _hmrcService.Verify(
                x => x.IsNationalInsuranceNumberRelatedToPayeScheme(paye.Id, command, command.Apprentice.StartDate,
                    command.Apprentice.EndDate), Times.Exactly(1));
        }

        [Fact]
        public async void Then_When_The_HmrcApi_Returns_True_No_Further_Checks_Take_Place()
        {
            //Arrange

            var command = new CheckApprenticeCommand(_apprentice);

            var sut = new CheckApprenticeCommandHandler(_employmentCheckService.Object, _accountsService.Object,
                _hmrcService.Object, _logger.Object);

            var paye1 = new ResourceViewModel();
            paye1.Id = "testId";
            var paye2 = new ResourceViewModel();
            paye2.Id = "notUsedTestId";
            var payes = new ResourceList(new List<ResourceViewModel> { paye1, paye2 });

            _accountsService.Setup(x => x.GetEmployerAccount(command.Apprentice.AccountId))
                .ReturnsAsync(new AccountDetailViewModel { PayeSchemes = payes });

            _hmrcService.Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme("testId", command,
                command.Apprentice.StartDate, command.Apprentice.EndDate)).ReturnsAsync(true);

            //Act

            await sut.Handle(command, CancellationToken.None);

            //Assert

            _hmrcService.Verify(
                x => x.IsNationalInsuranceNumberRelatedToPayeScheme(paye1.Id, command, command.Apprentice.StartDate,
                    command.Apprentice.EndDate), Times.Exactly(1));
            _hmrcService.Verify(
                x => x.IsNationalInsuranceNumberRelatedToPayeScheme(paye2.Id, command, command.Apprentice.StartDate,
                    command.Apprentice.EndDate), Times.Exactly(0));
        }

        [Fact]
        public async void And_The_HmrcService_Returns_False_For_Each_Paye_Scheme_Then_It_Is_Called_For_Each_Paye_Scheme()
        {
            //Arrange

            var command = new CheckApprenticeCommand(_apprentice);

            var sut = new CheckApprenticeCommandHandler(_employmentCheckService.Object, _accountsService.Object,
                _hmrcService.Object, _logger.Object);

            var paye1 = new ResourceViewModel();
            paye1.Id = "testId";
            var paye2 = new ResourceViewModel();
            paye2.Id = "notUsedTestId";
            var paye3 = new ResourceViewModel();
            paye3.Id = "badTestId";
            var payes = new ResourceList(new List<ResourceViewModel> { paye1, paye2, paye3 });

            _accountsService.Setup(x => x.GetEmployerAccount(command.Apprentice.AccountId))
                .ReturnsAsync(new AccountDetailViewModel { PayeSchemes = payes });

            //Act

            await sut.Handle(command, CancellationToken.None);

            //Assert

            _hmrcService.Verify(
                x => x.IsNationalInsuranceNumberRelatedToPayeScheme(It.IsAny<string>(), command, command.Apprentice.StartDate,
                    command.Apprentice.EndDate), Times.Exactly(payes.Count));
        }

        [Fact]
        public async void Then_The_Result_Is_Stored()
        {
            //Arrange

            var command = new CheckApprenticeCommand(_apprentice);

            var sut = new CheckApprenticeCommandHandler(_employmentCheckService.Object, _accountsService.Object,
                _hmrcService.Object, _logger.Object);

            var paye = new ResourceViewModel();
            paye.Id = "testId";
            var payes = new ResourceList(new List<ResourceViewModel> { paye });

            _accountsService.Setup(x => x.GetEmployerAccount(command.Apprentice.AccountId))
                .ReturnsAsync(new AccountDetailViewModel { PayeSchemes = payes });

            _hmrcService.Setup(x =>
                x.IsNationalInsuranceNumberRelatedToPayeScheme(paye.Id, command, command.Apprentice.StartDate,
                    command.Apprentice.EndDate)).ReturnsAsync(true);

            //Act

            await sut.Handle(command, CancellationToken.None);

            //Assert

            _employmentCheckService.Verify(
                x => x.SaveEmploymentCheckResult(command.Apprentice.Id, command.Apprentice.ULN, true),
                Times.Exactly(1));
        }
    }
}