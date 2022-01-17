using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmployerAccount;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Clients.EmployerAccount.EmployerAccountClientTests
{
    public class WhenGettingEmployersPayeSchemes
    {
        private Mock<IEmployerAccountService> _employerAccountService;
        private Mock<ILogger<IEmploymentCheckClient>> _logger;
        private List<Functions.Application.Models.EmploymentCheck> _employmentCheckBatch;
        private Fixture _fixture;
        private EmployerAccountClient _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employerAccountService = new Mock<IEmployerAccountService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
            _employmentCheckBatch = new List<Functions.Application.Models.EmploymentCheck> {_fixture.Create<Functions.Application.Models.EmploymentCheck>()};
        }

        [Test]
        public async Task Then_The_EmployerAccountService_Is_Called()
        {
            //Arrange
            _employerAccountService.Setup(x => x.GetEmployerPayeSchemes(_employmentCheckBatch[0]))
                .ReturnsAsync(_fixture.Create<EmployerPayeSchemes>());

            var sut = new EmployerAccountClient(_employerAccountService.Object);

            //Act
            await sut.GetEmployersPayeSchemes(_employmentCheckBatch);

            //Assert
            _employerAccountService.Verify(x => x.GetEmployerPayeSchemes(_employmentCheckBatch[0]), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmployerAccountService_Returns_Paye_Scheme_Then_It_Is_Returned_Uppercased()
        {
            //Arrange
            var employerPayeSchemes = new EmployerPayeSchemes
            {
                EmployerAccountId = 1,
                PayeSchemes = new List<string>()
            };

            _employerAccountService.Setup(x => x.GetEmployerPayeSchemes(_employmentCheckBatch[0]))
                .ReturnsAsync(employerPayeSchemes);

            var sut = new EmployerAccountClient(_employerAccountService.Object);


            //Act
            var result = await sut.GetEmployersPayeSchemes(_employmentCheckBatch);

            //Assert
            result.First().PayeSchemes.First().Should().BeEquivalentTo(employerPayeSchemes.PayeSchemes.First());
        }

        [Test]
        public async Task And_No_Learners_Are_Passed_In_Then_An_Empty_List_Is_Returned()
        {
            //Act
            var result = await _sut.GetEmployersPayeSchemes(new List<Functions.Application.Models.EmploymentCheck>());

            var sut = new EmployerAccountClient(_employerAccountService.Object);
        }

        [Test]
        public async Task And_Null_Is_Passed_In_Then_An_Empty_List_Is_Returned()
        {
            //Act
            var result = await _sut.GetEmployersPayeSchemes(null);

            //Assert
            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }


        [Test]
        public async Task And_The_EmployerAccountService_Returns_Paye_Scheme_Then_It_Is_Returned_UpperCased_and_Trimmed_of_WhiteSpace()
        {
            //Arrange
            var employerPayeSchemes = new EmployerPayeSchemes
            {
                EmployerAccountId = 1,
                PayeSchemes = new List<string>()
            };

            _employerAccountService.Setup(x => x.GetEmployerPayeSchemes(_employmentCheckBatch[0]))
                .ReturnsAsync(employerPayeSchemes);

            //Act
            var result = await _sut.GetEmployersPayeSchemes(_employmentCheckBatch);

            //Assert
            result.First().PayeSchemes.First().Should().BeEquivalentTo("PAYE");
        }
    }
}