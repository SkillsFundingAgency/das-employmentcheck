using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Clients.EmployerAccount.EmployerAccountClientTests
{
    public class WhenGettingEmployersPayeSchemes
    {
        private readonly Mock<IEmployerAccountService> _employerAccountService;
        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;
        private readonly List<ApprenticeEmploymentCheckModel> _apprentices;
        private readonly Fixture _fixture;

        public WhenGettingEmployersPayeSchemes()
        {
            _fixture = new Fixture();
            _employerAccountService = new Mock<IEmployerAccountService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
            _apprentices = new List<ApprenticeEmploymentCheckModel> {_fixture.Create<ApprenticeEmploymentCheckModel>()};
        }

        [Fact]
        public async Task Then_The_EmployerAccountService_Is_Called()
        {
            //Arrange
            _employerAccountService.Setup(x => x.GetEmployerAccount(_apprentices[0].AccountId))
                .ReturnsAsync(_fixture.Create<ResourceList>());

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            await sut.GetEmployersPayeSchemes(_apprentices);

            //Assert

            _employerAccountService.Verify(x => x.GetEmployerAccount(_apprentices[0].AccountId), Times.Exactly(1));
        }

        [Fact]
        public async Task And_The_EmployerAccountService_Returns_Paye_Scheme_Then_It_Is_Returned_Uppercased()
        {
            //Arrange
            var resource = new ResourceViewModel
            {
                Href = "href",
                Id = "id"
            };

            var accountDetail = new ResourceList(new List<ResourceViewModel> { resource });

            _employerAccountService.Setup(x => x.GetEmployerAccount(_apprentices[0].AccountId))
                .ReturnsAsync(accountDetail);

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(_apprentices);

            //Assert

            result.First().PayeSchemes.First().Should().BeEquivalentTo(resource.Id.ToUpper());
        }

        [Fact]
        public async Task And_No_Apprentices_Are_Passed_In_Then_An_Empty_List_Is_Returned()
        {
            //Arrange

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(new List<ApprenticeEmploymentCheckModel>());

            //Assert

            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }

        [Fact]
        public async Task And_Null_Is_Passed_In_Then_An_Empty_List_Is_Returned()
        {
            //Arrange

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(null);

            //Assert

            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }

        [Fact]
        public async Task And_An_Exception_Is_Thrown_Then_An_Empty_List_Is_Returned()
        {
            //Arrange

            var exception = new Exception("exception");

            _employerAccountService.Setup(x => x.GetEmployerAccount(It.IsAny<long>())).ThrowsAsync(exception);

            var sut = new EmployerAccountClient(_employerAccountService.Object, _logger.Object);

            //Act

            var result = await sut.GetEmployersPayeSchemes(_apprentices);

            //Assert

            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }
    }
}