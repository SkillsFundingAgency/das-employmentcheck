using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetEmployerPayeSchemes.GetEmployerPayeSchemesMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private readonly Mock<IEmployerAccountClient> _employerAccountClient;
        private readonly Mock<ILogger<GetEmployerPayeSchemesMediatorHandler>> _logger;

        public WhenHandlingTheRequest()
        {
            _employerAccountClient = new Mock<IEmployerAccountClient>();
            _logger = new Mock<ILogger<GetEmployerPayeSchemesMediatorHandler>>();
        }

        [Fact]
        public async void And_There_Are_No_Apprentices_Then_That_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetEmployersPayeSchemesMediatorRequest(new List<Apprentice>()),
                CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation("ERROR - apprentices parameter is null, no employer PAYE schemes were retrieved"));
            Assert.Equal(new List<EmployerPayeSchemes>(), result.EmployersPayeSchemes);
        }

        [Fact]
        public async void And_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_They_Are_Logged_And_Returned()
        {
            //Arrange

            var apprentice = new Apprentice(
                1,
                1,
                "1000001",
                1000001,
                1000001,
                1,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
            var apprentices = new List<Apprentice> {apprentice};
            var request = new GetEmployersPayeSchemesMediatorRequest(apprentices);

            var payeScheme = new EmployerPayeSchemes(1, new List<string> {"paye scheme"});
            var payeSchemes = new List<EmployerPayeSchemes> {payeScheme};

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.Apprentices)).ReturnsAsync(payeSchemes);

            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation($"GetEmployerPayeSchemesMediatorHandler.Handle() returned {payeSchemes.Count} PAYE scheme(s)"));
            Assert.Equal(result.EmployersPayeSchemes, payeSchemes);
        }

        [Fact]
        public async void And_No_Paye_Schemes_Are_Returned_From_The_EmployerAccountClient_Then_That_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var apprentice = new Apprentice(
                1,
                1,
                "1000001",
                1000001,
                1000001,
                1,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
            var apprentices = new List<Apprentice> { apprentice };
            var request = new GetEmployersPayeSchemesMediatorRequest(apprentices);

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.Apprentices)).ReturnsAsync(new List<EmployerPayeSchemes>());

            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation("GetEmployerPayeSchemesMediatorHandler.Handle() returned null/zero PAYE schemes"));
            result.EmployersPayeSchemes.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }

        [Fact]
        public async void And_The_EmployerAccountClient_Returns_Null_Then_That_Is_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var apprentice = new Apprentice(
                1,
                1,
                "1000001",
                1000001,
                1000001,
                1,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
            var apprentices = new List<Apprentice> { apprentice };
            var request = new GetEmployersPayeSchemesMediatorRequest(apprentices);

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.Apprentices)).ReturnsAsync((List<EmployerPayeSchemes>)null);

            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation("GetEmployerPayeSchemesMediatorHandler.Handle() returned null/zero PAYE schemes"));
            result.EmployersPayeSchemes.Should().BeEquivalentTo(new List<EmployerPayeSchemes>());
        }

        [Fact]
        public async void And_The_EmployerAccountClient_Throws_An_Exception_Then_That_Is_Logged()
        {
            //Arrange

            var apprentice = new Apprentice(
                1,
                1,
                "1000001",
                1000001,
                1000001,
                1,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
            var apprentices = new List<Apprentice> { apprentice };
            var request = new GetEmployersPayeSchemesMediatorRequest(apprentices);

            var exception = new Exception("exception");

            _employerAccountClient.Setup(x => x.GetEmployersPayeSchemes(request.Apprentices)).ThrowsAsync(exception);

            var sut = new GetEmployerPayeSchemesMediatorHandler(_employerAccountClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(request, CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation($"\n\nGetEmployerPayeSchemesMediatorHandler.Handle(): Exception caught - {exception.Message}. {exception.StackTrace}"));
        }
    }
}