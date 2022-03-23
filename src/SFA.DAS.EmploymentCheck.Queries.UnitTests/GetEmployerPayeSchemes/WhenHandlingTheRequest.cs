using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetEmployerPayeSchemes
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<IEmployerAccountService> _employerAccountService;
        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _employerAccountService = new Mock<IEmployerAccountService>();
            _fixture = new Fixture();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
        }

        [Test]
        public async Task And_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_They_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetPayeSchemesQueryRequest>();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();

            _employerAccountService
                .Setup(x => x.GetEmployerPayeSchemes(request.EmploymentCheck))
                .ReturnsAsync(payeScheme);

            var sut = new GetPayeSchemeQueryHandler(_employerAccountService.Object);

            // Act
            var result = await sut.Handle(request, CancellationToken.None);

            // Assert
            Assert.AreEqual(result.EmployersPayeSchemes, payeScheme);
        }

        [Test]
        public async Task And_No_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetPayeSchemesQueryRequest>();

            _employerAccountService
                .Setup(x => x.GetEmployerPayeSchemes(request.EmploymentCheck))
                .ReturnsAsync(new EmployerPayeSchemes(request.EmploymentCheck.AccountId, HttpStatusCode.OK));

            var sut = new GetPayeSchemeQueryHandler(_employerAccountService.Object);

            // Act
            var result = await sut.Handle(request, CancellationToken.None);

            // Assert
            result.EmployersPayeSchemes.PayeSchemes.Count.Should().Be(0);
        }
    }
}