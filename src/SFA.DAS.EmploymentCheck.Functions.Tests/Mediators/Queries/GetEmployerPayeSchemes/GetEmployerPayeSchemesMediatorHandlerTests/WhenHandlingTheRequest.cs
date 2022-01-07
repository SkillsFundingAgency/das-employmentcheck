using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetEmployerPayeSchemes.GetEmployerPayeSchemesMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private readonly Mock<IEmployerAccountClient> _employerAccountClient;
        private readonly Mock<ILogger<GetPayeSchemesQueryHandler>> _logger;
        private readonly List<Functions.Application.Models.EmploymentCheck> _apprentices;

        public WhenHandlingTheRequest()
        {
            _employerAccountClient = new Mock<IEmployerAccountClient>();
            _logger = new Mock<ILogger<GetPayeSchemesQueryHandler>>();
            var fixture = new Fixture();
            _apprentices = fixture.CreateMany<Functions.Application.Models.EmploymentCheck>().ToList();
        }

        [Fact]
        public async Task And_There_Are_No_Learners_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var sut = new GetPayeSchemesQueryHandler(_logger.Object, _employerAccountClient.Object);

            //Act
            var result = await sut.Handle(new GetPayeSchemesQueryRequest(new List<Functions.Application.Models.EmploymentCheck>()),
                CancellationToken.None);

            //Assert
            result.EmployersPayeSchemes.Should().BeNull();
        }

        [Fact]
        public async Task And_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_They_Returned()
        {
            //Arrange
            var request = new GetPayeSchemesQueryRequest(_apprentices);
            var payeScheme = new EmployerPayeSchemes(1, new List<string> { "paye scheme" });
            var payeSchemes = new List<EmployerPayeSchemes> { payeScheme };

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.EmploymentCheckBatch)).ReturnsAsync(payeSchemes);

            var sut = new GetPayeSchemesQueryHandler(_logger.Object, _employerAccountClient.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            Assert.Equal(result.EmployersPayeSchemes, payeSchemes);
        }

        [Fact]
        public async Task And_No_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var request = new GetPayeSchemesQueryRequest(_apprentices);

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.EmploymentCheckBatch)).ReturnsAsync(new List<EmployerPayeSchemes>());
          
            var sut = new GetPayeSchemesQueryHandler(_logger.Object, _employerAccountClient.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            result.EmployersPayeSchemes.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }

        [Fact]
        public async Task And_The_EmployerAccountClient_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var request = new GetPayeSchemesQueryRequest(_apprentices);

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.EmploymentCheckBatch)).ReturnsAsync((List<EmployerPayeSchemes>)null);

            var sut = new GetPayeSchemesQueryHandler(_logger.Object, _employerAccountClient.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            result.EmployersPayeSchemes.Should().BeNull();
        }
    }
}