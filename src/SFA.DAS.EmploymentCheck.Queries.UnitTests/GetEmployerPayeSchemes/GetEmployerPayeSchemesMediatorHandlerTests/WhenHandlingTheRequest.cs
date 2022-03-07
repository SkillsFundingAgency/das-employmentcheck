using System.Collections.Generic;
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
        private Mock<ILogger<GetPayeSchemeQueryHandler>> _logger;
        private Data.Models.EmploymentCheck _employmentCheck;

        [SetUp]
        public void SetUp()
        {
            _employerAccountClient = new Mock<IEmployerAccountClient>();
            _logger = new Mock<ILogger<GetPayeSchemeQueryHandler>>();
            _fixture = new Fixture();
            _employmentCheck = _fixture.Create<Data.Models.EmploymentCheck>();
        }

        [Test]
        public async Task And_There_Are_No_Learners_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetPayeSchemesQueryRequest>();
            var payeSchemes = _fixture.Create<EmployerPayeSchemes>();

            _employerAccountClient
                .Setup(x => x.GetEmployersPayeSchemes(request.EmploymentCheck))
                .ReturnsAsync(payeSchemes);

            var sut = new GetPayeSchemeQueryHandler(_logger.Object, _employerAccountClient.Object);

            // Act
            var result = await sut.Handle(new GetPayeSchemesQueryRequest(new Data.Models.EmploymentCheck()),
                CancellationToken.None);

            // Assert
            result.EmployersPayeSchemes.Should().BeEquivalentTo(new EmployerPayeSchemes());
        }

        [Test]
        public async Task And_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_They_Returned()
        {
            // Arrange
            var request = new GetPayeSchemesQueryRequest(_employmentCheck);
            var payeScheme = new EmployerPayeSchemes(1, new List<string> { "paye scheme" }, System.Net.HttpStatusCode.OK);

            _employerAccountClient
                .Setup(x => x.GetEmployersPayeSchemes(request.EmploymentCheck))
                .ReturnsAsync(payeScheme);

            var sut = new GetPayeSchemeQueryHandler(_logger.Object, _employerAccountClient.Object);

            // Act

            var result = await sut.Handle(request, CancellationToken.None);

            // Assert

            Assert.AreEqual(result.EmployersPayeSchemes, payeScheme);
        }

        [Test]
        public async Task And_No_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            var request = new GetPayeSchemesQueryRequest(_employmentCheck);

            _employerAccountClient
                .Setup(x => x.GetEmployersPayeSchemes(request.EmploymentCheck))
                .ReturnsAsync(new EmployerPayeSchemes());

            var sut = new GetPayeSchemeQueryHandler(_logger.Object, _employerAccountClient.Object);

            // Act
            var result = await sut.Handle(request, CancellationToken.None);

            // Assert
            result.EmployersPayeSchemes.Should().BeEquivalentTo(new EmployerPayeSchemes());
        }
    }
}