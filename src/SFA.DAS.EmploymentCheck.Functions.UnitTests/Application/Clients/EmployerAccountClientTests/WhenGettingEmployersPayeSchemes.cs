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

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmployerAccountClientTests
{
    public class WhenGettingEmployersPayeSchemes
    {
        private Fixture _fixture;
        private Mock<IEmployerAccountService> _employerAccountService;
        private Mock<ILogger<IEmploymentCheckClient>> _logger;

        private List<Functions.Application.Models.EmploymentCheck> _apprentices;
        private EmployerAccountClient _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employerAccountService = new Mock<IEmployerAccountService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();

            _apprentices = new List<Functions.Application.Models.EmploymentCheck>
            {
                _fixture.Create<Functions.Application.Models.EmploymentCheck>()
            };

            _sut = new EmployerAccountClient(_employerAccountService.Object);
        }

        [Test]
        public async Task Then_The_EmployerAccountService_Is_Called()
        {
            // Arrange
            _employerAccountService
                .Setup(x => x.GetEmployerPayeSchemes(_apprentices[0]))
                .ReturnsAsync(_fixture.Create<EmployerPayeSchemes>());

            // Act
            await _sut.GetEmployersPayeSchemes(_apprentices);

            // Assert
            _employerAccountService.Verify(x => x.GetEmployerPayeSchemes(_apprentices[0]), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmployerAccountService_Returns_Paye_Scheme_Then_It_Is_Returned_Uppercased()
        {
            // Arrange
            var employerPayeSchemes = _fixture
                .Build<EmployerPayeSchemes>()
                .With(p => p.EmployerAccountId, 1)
                .With(p => p.PayeSchemes, new List<string>{"paye"})
                .Create();

            _employerAccountService
                .Setup(x => x.GetEmployerPayeSchemes(_apprentices[0]))
                .ReturnsAsync(employerPayeSchemes);

            // Act
            var result = await _sut.GetEmployersPayeSchemes(_apprentices);

            // Assert
            result.First().PayeSchemes.First().Should().BeEquivalentTo(employerPayeSchemes.PayeSchemes.First());
        }

        [Test]
        public async Task And_No_Learners_Are_Passed_In_Then_An_Empty_List_Is_Returned()
        {
            // Act
            var result = await _sut.GetEmployersPayeSchemes(new List<Functions.Application.Models.EmploymentCheck>());

            //Assert
            result.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }

        [Test]
        public void And_Null_Is_Passed_In_Then_A_NullReference_Exception_Occurs()
        {
            // Assert
            Assert.ThrowsAsync<NullReferenceException>(async () => await _sut.GetEmployersPayeSchemes(null));
        }
    }
}