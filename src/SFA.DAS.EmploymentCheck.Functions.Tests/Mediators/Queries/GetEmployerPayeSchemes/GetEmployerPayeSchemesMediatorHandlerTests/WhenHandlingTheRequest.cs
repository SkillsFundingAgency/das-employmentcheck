using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes;
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
        private readonly Mock<ILogger<GetEmployerPayeSchemesMediatorHandler>> _logger;
        private readonly List<ApprenticeEmploymentCheckModel> _apprentices;

        public WhenHandlingTheRequest()
        {
            _employerAccountClient = new Mock<IEmployerAccountClient>();
            _logger = new Mock<ILogger<GetEmployerPayeSchemesMediatorHandler>>();
            var fixture = new Fixture();
            _apprentices = fixture.CreateMany<ApprenticeEmploymentCheckModel>().ToList();
        }

        [Fact]
        public async Task And_There_Are_No_Apprentices_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act
            var result = await sut.Handle(new GetEmployersPayeSchemesMediatorRequest(new List<ApprenticeEmploymentCheckModel>()),
                CancellationToken.None);

            //Assert
            Assert.Equal(new List<EmployerPayeSchemes>(), result.EmployersPayeSchemes);
        }

        [Fact]
        public async Task And_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_They_Returned()
        {
            //Arrange
            var request = new GetEmployersPayeSchemesMediatorRequest(_apprentices);
            var payeScheme = new EmployerPayeSchemes(1, new List<string> { "paye scheme" });
            var payeSchemes = new List<EmployerPayeSchemes> { payeScheme };

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.Apprentices)).ReturnsAsync(payeSchemes);

            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            Assert.Equal(result.EmployersPayeSchemes, payeSchemes);
        }

        [Fact]
        public async Task And_No_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var request = new GetEmployersPayeSchemesMediatorRequest(_apprentices);

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.Apprentices)).ReturnsAsync(new List<EmployerPayeSchemes>());

            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            result.EmployersPayeSchemes.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }

        [Fact]
        public async Task And_The_EmployerAccountClient_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var request = new GetEmployersPayeSchemesMediatorRequest(_apprentices);

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.Apprentices)).ReturnsAsync((List<EmployerPayeSchemes>)null);

            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            result.EmployersPayeSchemes.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }
    }
}