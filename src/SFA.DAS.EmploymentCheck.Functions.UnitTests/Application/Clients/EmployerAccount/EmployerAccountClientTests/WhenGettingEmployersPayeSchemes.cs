using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmployerAccount.EmployerAccountClientTests
{
    public class WhenGettingEmployersPayeSchemes
    {
        private Mock<IEmployerAccountService> _employerAccountService;
        private Mock<ILogger<IEmploymentCheckClient>> _logger;
        private Models.EmploymentCheck _employmentCheck;
        private Fixture _fixture;
        private EmployerAccountClient _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employerAccountService = new Mock<IEmployerAccountService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
            _employmentCheck = _fixture.Create<Functions.Application.Models.EmploymentCheck>();

            _sut = new EmployerAccountClient(_employerAccountService.Object);
        }

        [Test]
        public async Task Then_The_EmployerAccountService_Is_Called()
        {
            //Arrange
            _employerAccountService.Setup(x => x.GetEmployerPayeSchemes(_employmentCheck))
                .ReturnsAsync(_fixture.Create<EmployerPayeSchemes>());

            //Act
            await _sut.GetEmployersPayeSchemes(_employmentCheck);

            //Assert
            _employerAccountService.Verify(x => x.GetEmployerPayeSchemes(_employmentCheck), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmployerAccountService_Returns_Paye_Scheme_Then_It_Is_Returned_Uppercased()
        {
            //Arrange
            var employerPayeSchemes = new EmployerPayeSchemes
            {
                EmployerAccountId = 1,
                PayeSchemes = new List<string> { " paye " }
            };

            _employerAccountService.Setup(x => x.GetEmployerPayeSchemes(_employmentCheck))
                .ReturnsAsync(employerPayeSchemes);

            //Act
            var result = await _sut.GetEmployersPayeSchemes(_employmentCheck);

            //Assert
            result.PayeSchemes.First().Should().BeEquivalentTo(employerPayeSchemes.PayeSchemes.First());
        }

        [Test]
        public async Task And_No_Learners_Are_Passed_In_Then_An_Empty_List_Is_Returned()
        {
            //Act
            var result = await _sut.GetEmployersPayeSchemes(new Models.EmploymentCheck());

            //Assert
            result.Should().BeEquivalentTo(new EmployerPayeSchemes());
        }

        [Test]
        public async Task And_Null_Is_Passed_In_An_Empty_List_Is_Returned()
        {
            //Act
            var result = await _sut.GetEmployersPayeSchemes(null);

            //Assert
            result.Should().BeEquivalentTo(new EmployerPayeSchemes());
        }
    }
}