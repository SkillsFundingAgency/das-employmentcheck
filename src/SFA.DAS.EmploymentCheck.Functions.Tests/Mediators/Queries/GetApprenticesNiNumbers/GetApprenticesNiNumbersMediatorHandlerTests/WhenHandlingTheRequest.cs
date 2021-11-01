using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesNiNumbers.GetApprenticesNiNumbersMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private readonly Mock<ISubmitLearnerDataClient> _submitLearnerDataClient;
        private readonly Mock<ILoggerAdapter<GetApprenticesNiNumbersMediatorHandler>> _logger;

        public WhenHandlingTheRequest()
        {
            _submitLearnerDataClient = new Mock<ISubmitLearnerDataClient>();
            _logger = new Mock<ILoggerAdapter<GetApprenticesNiNumbersMediatorHandler>>();
        }

        [Fact]
        public async void Then_The_SubmitLearnerDataClient_Is_Called()
        {
            //Arrange
            var request = new GetApprenticesNiNumberMediatorRequest(new List<Apprentice>());

            _submitLearnerDataClient.Setup(x => x.GetApprenticesNiNumber(request.Apprentices))
                .ReturnsAsync(new List<ApprenticeNiNumber>());

            var sut = new GetApprenticesNiNumbersMediatorHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesNiNumberMediatorRequest(new List<Apprentice>()), CancellationToken.None);

            //Assert

            _submitLearnerDataClient.Verify(x => x.GetApprenticesNiNumber(request.Apprentices), Times.Exactly(1));
        }

        [Fact]
        public async void And_The_SubmitLearnerDataClient_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var request = new GetApprenticesNiNumberMediatorRequest(new List<Apprentice>());

            _submitLearnerDataClient.Setup(x => x.GetApprenticesNiNumber(request.Apprentices))
                .ReturnsAsync((List<ApprenticeNiNumber>)null);

            var sut = new GetApprenticesNiNumbersMediatorHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesNiNumberMediatorRequest(new List<Apprentice>()), CancellationToken.None);

            //Assert

            result.ApprenticesNiNumber.Should().BeEquivalentTo(new List<ApprenticeNiNumber>());
        }

        [Fact]
        public async void And_The_SubmitLearnerDataClient_Returns_ApprenticeNiNumbers_Then_They_Are_Returned()
        {
            //Arrange
            var request = new GetApprenticesNiNumberMediatorRequest(new List<Apprentice>());

            var niNumber = new ApprenticeNiNumber(1000001, "1000001");
            var niNumbers = new List<ApprenticeNiNumber> {niNumber};

            _submitLearnerDataClient.Setup(x => x.GetApprenticesNiNumber(request.Apprentices))
                .ReturnsAsync(niNumbers);

            var sut = new GetApprenticesNiNumbersMediatorHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesNiNumberMediatorRequest(new List<Apprentice>()), CancellationToken.None);

            //Assert

            result.ApprenticesNiNumber.Should().BeEquivalentTo(niNumbers);
        }

        [Fact]
        public async void And_The_SumbitLearnerDataClient_Throws_And_Exception_Then_It_Is_Logged()
        {
            //Arrange

            var request = new GetApprenticesNiNumberMediatorRequest(new List<Apprentice>());

            var exception = new Exception("Exception");
            _submitLearnerDataClient.Setup(x => x.GetApprenticesNiNumber(request.Apprentices)).ThrowsAsync(exception);

            var sut = new GetApprenticesNiNumbersMediatorHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            await sut.Handle(request, CancellationToken.None);

            //Assert

            _logger.Verify(x => x.LogInformation($"\n\nGetApprenticesNiNumbersHandler.Handle(): Exception caught - {exception.Message}. {exception.StackTrace}"));
        }
    }
}