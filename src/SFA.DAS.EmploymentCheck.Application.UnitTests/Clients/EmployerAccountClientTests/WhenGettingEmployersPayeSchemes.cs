using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Clients.EmployerAccountClientTests
{
    public class WhenGettingEmployersPayeSchemes
    {
        private Fixture _fixture;
        private Mock<IEmployerAccountService> _employerAccountService;
        private Data.Models.EmploymentCheck _employmentCheck;
        private EmployerAccountClient _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employerAccountService = new Mock<IEmployerAccountService>();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();

            _sut = new EmployerAccountClient(_employerAccountService.Object);
        }

        [Test]
        public async Task Then_The_EmployerAccountService_Is_Called()
        {
            // Arrange
            _employerAccountService
                .Setup(x => x.GetEmployerPayeSchemes(_employmentCheck))
                .ReturnsAsync(_fixture.Create<EmployerPayeSchemes>());

            // Act
            await _sut.GetEmployersPayeSchemes(_employmentCheck);

            // Assert
            _employerAccountService.Verify(x => x.GetEmployerPayeSchemes(_employmentCheck), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_EmployerAccountService_Returns_Paye_Scheme_Then_It_Is_Returned_Uppercased()
        {
            // Arrange
            var input = _fixture.Build<EmployerPayeSchemes>().With(p => p.EmployerAccountId, 1).With(p => p.PayeSchemes, new List<string> { "paye" }).Create();
            var expected = _fixture.Build<EmployerPayeSchemes>().With(p => p.EmployerAccountId, 1).With(p => p.PayeSchemes, new List<string> { "PAYE" }).Create();

            _employerAccountService
                .Setup(x => x.GetEmployerPayeSchemes(_employmentCheck))
                .ReturnsAsync(input);

            // Act
            var result = await _sut.GetEmployersPayeSchemes(_employmentCheck);

            // Assert
            result.PayeSchemes.First().Should().BeEquivalentTo(expected.PayeSchemes.First());
        }

        [Test]
        public async Task And_Null_Is_Passed_In_The_Then_An_Empty_Paye_Scheme_Is_Returned()
        {
            // Act
            var result = await _sut.GetEmployersPayeSchemes(new Data.Models.EmploymentCheck());

            // Assert
            result.Should().BeEquivalentTo(new EmployerPayeSchemes());
        }
    }
}