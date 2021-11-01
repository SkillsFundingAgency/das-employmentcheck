using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprentices;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprentices.GetApprenticesMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private readonly Mock<IEmploymentCheckClient> _employmentCheckClient;
        private readonly Mock<ILoggerAdapter<GetApprenticesMediatorHandler>> _logger;

        public WhenHandlingTheRequest()
        {
            _employmentCheckClient = new Mock<IEmploymentCheckClient>();
            _logger = new Mock<ILoggerAdapter<GetApprenticesMediatorHandler>>();
        }

        [Fact]
        public async void Then_The_EmploymentCheckClient_Is_Called()
        {
            //Arrange

            _employmentCheckClient.Setup(x => x.GetApprentices()).ReturnsAsync(new List<Apprentice>());

            var sut = new GetApprenticesMediatorHandler(_employmentCheckClient.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesMediatorRequest(), CancellationToken.None);

            //Assert

            _employmentCheckClient.Verify(x => x.GetApprentices(), Times.Exactly(1));
        }

        [Fact]
        public async void And_No_Apprentices_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Logged()
        {
            //Arrange

            _employmentCheckClient.Setup(x => x.GetApprentices()).ReturnsAsync(new List<Apprentice>());

            var sut = new GetApprenticesMediatorHandler(_employmentCheckClient.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesMediatorRequest(), CancellationToken.None);

            //Assert
            
            _logger.Verify(x => x.LogInformation("GetApprenticesMediatorHandler.Handle() returned null/zero apprentices"));
        }

        [Fact]
        public async void And_Null_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Logged()
        {
            //Arrange

            _employmentCheckClient.Setup(x => x.GetApprentices()).ReturnsAsync((List<Apprentice>)null);

            var sut = new GetApprenticesMediatorHandler(_employmentCheckClient.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesMediatorRequest(), CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation("GetApprenticesMediatorHandler.Handle() returned null/zero apprentices"));
        }

        [Fact]
        public async void And_Apprentices_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Logged()
        {
            //Arrange

            var apprentice = new Apprentice(1,
                1,
                "1000001",
                1000001,
                1000001,
                1,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
            var apprentices = new List<Apprentice> {apprentice};

            _employmentCheckClient.Setup(x => x.GetApprentices()).ReturnsAsync(apprentices);

            var sut = new GetApprenticesMediatorHandler(_employmentCheckClient.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesMediatorRequest(), CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation($"GetApprenticesMediatorHandler.Handle() returned {apprentices.Count} apprentice(s)"));
        }

        [Fact]
        public async void And_Apprentices_Returned_From_The_EmploymentcheckClient_Then_Result_Is_Returned()
        {
            //Arrange

            var apprentice = new Apprentice(1,
                1,
                "1000001",
                1000001,
                1000001,
                1,
                DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
            var apprentices = new List<Apprentice> { apprentice };

            _employmentCheckClient.Setup(x => x.GetApprentices()).ReturnsAsync(apprentices);

            var expected = new GetApprenticesMediatorResult(apprentices);

            var sut = new GetApprenticesMediatorHandler(_employmentCheckClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesMediatorRequest(), CancellationToken.None);

            //Assert

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async void
            And_The_EmploymentCheckClient_Throws_An_Exception_Then_It_It_Logged_And_An_Empty_List_Is_Returned()
        {
            //Arrange

            var exception = new Exception("Exception");

            _employmentCheckClient.Setup(x => x.GetApprentices()).ThrowsAsync(exception);

            var sut = new GetApprenticesMediatorHandler(_employmentCheckClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesMediatorRequest(), CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation($"\n\nGetApprenticesMediatorHandler.Handle(): Exception caught - {exception.Message}. {exception.StackTrace}"));
            result.Should().BeEquivalentTo(new GetApprenticesMediatorResult(null));
        }
    }
}