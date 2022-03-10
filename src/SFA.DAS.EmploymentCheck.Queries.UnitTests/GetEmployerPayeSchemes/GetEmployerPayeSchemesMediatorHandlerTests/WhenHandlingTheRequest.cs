using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetEmployerPayeSchemes.GetEmployerPayeSchemesMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<IEmployerAccountClient> _employerAccountClient;

        [SetUp]
        public void SetUp()
        {
            _employerAccountClient = new Mock<IEmployerAccountClient>();
            _fixture = new Fixture();
        }

        [Test]
        public async Task And_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_They_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetPayeSchemesQueryRequest>();
            var payeScheme = _fixture.Create<EmployerPayeSchemes>();

            _employerAccountClient
                .Setup(x => x.GetEmployersPayeSchemes(request.EmploymentCheck))
                .ReturnsAsync(payeScheme);

            var sut = new GetPayeSchemeQueryHandler(_employerAccountClient.Object);

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

            _employerAccountClient
                .Setup(x => x.GetEmployersPayeSchemes(request.EmploymentCheck))
                .ReturnsAsync(new EmployerPayeSchemes(request.EmploymentCheck.AccountId, HttpStatusCode.NoContent));

            var sut = new GetPayeSchemeQueryHandler(_employerAccountClient.Object);

            // Act
            var result = await sut.Handle(request, CancellationToken.None);

            // Assert
            result.EmployersPayeSchemes.PayeSchemes.Count.Should().Be(0);
        }
    }
}