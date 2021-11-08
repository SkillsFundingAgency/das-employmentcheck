using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Clients.EmployerAccount.EmployerAccountClientTests
{
    public class WhenGettingEmployersPayeSchemes
    {
        private readonly Mock<IEmployerAccountService> _employerAccountService;
        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;
        private readonly List<Apprentice> _apprentices;

        public WhenGettingEmployersPayeSchemes()
        {
            _employerAccountService = new Mock<IEmployerAccountService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();

            var apprentice = new Apprentice(
                1,
                1,
                "1000001",
                1000001,
                1000001,
                1,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));

            _apprentices = new List<Apprentice> {apprentice};
        }

        [Fact]
        public async void Then_The_EmployerAccountService_Is_Called()
        {
            //Arrange

            _employerAccountService.Setup(x => x.GetEmployerAccount(_apprentices[0].AccountId))
                .ReturnsAsync(new AccountDetailViewModel());

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            await sut.GetEmployersPayeSchemes(_apprentices);

            //Assert

            _employerAccountService.Verify(x => x.GetEmployerAccount(_apprentices[0].AccountId), Times.Exactly(1));
        }

        [Fact]
        public async void And_The_EmployerAccountService_Returns_Paye_Scheme_Then_It_Is_Returned()
        {
            //Arrange

            var resource = new ResourceViewModel
            {
                Href = "href",
                Id = "id"
            };

            var accountDetail = new AccountDetailViewModel{PayeSchemes = new ResourceList(new List<ResourceViewModel> {resource})};

            _employerAccountService.Setup(x => x.GetEmployerAccount(_apprentices[0].AccountId))
                .ReturnsAsync(accountDetail);

            var expected = new List<EmployerPayeSchemes>
                {new EmployerPayeSchemes(_apprentices[0].AccountId, new List<string> {resource.Id})};

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(_apprentices);

            //Assert

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async void
            And_The_EmployerAccountsService_Returns_No_Paye_Schemes_Then_It_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            _employerAccountService.Setup(x => x.GetEmployerAccount(_apprentices[0].AccountId))
                .ReturnsAsync(new AccountDetailViewModel());

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(_apprentices);
            
            //Assert

            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
            _logger.Verify(x => x.LogInformation("GetApprenticesNiNumberClient.Get(): ERROR: AccountDetailViewModel/PayeSchemes parameter is NULL, no employer PAYE schemes retrieved"));
        }

        [Fact]
        public async void And_No_Apprentices_Are_Passed_In_Then_It_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(new List<Apprentice>());

            //Assert

            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
            _logger.Verify(x => x.LogInformation("GetApprenticesNiNumberClient.Get(): ERROR: apprentices parameter is NULL, no employer PAYE schemes retrieved"));
        }

        [Fact]
        public async void And_Null_Is_Passed_In_Then_It_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(null);

            //Assert

            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
            _logger.Verify(x => x.LogInformation("GetApprenticesNiNumberClient.Get(): ERROR: apprentices parameter is NULL, no employer PAYE schemes retrieved"));
        }

        [Fact]
        public async void And_An_Exception_Is_Thrown_Then_It_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var exception = new Exception("exception");

            _employerAccountService.Setup(x => x.GetEmployerAccount(It.IsAny<long>())).ThrowsAsync(exception);

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(_apprentices);

            //Assert

            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
            _logger.Verify(x => x.LogInformation($"\n\nGetApprenticesNiNumberClient.Get(): Exception caught - {exception.Message}. {exception.StackTrace}"));
        }
    }
}