using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesNiNumbers.GetApprenticesNiNumbersMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private readonly Mock<ISubmitLearnerDataClient> _submitLearnerDataClient;
        private readonly Mock<ILogger<GetApprenticesNiNumbersMediatorHandler>> _logger;

        public WhenHandlingTheRequest()
        {
            _submitLearnerDataClient = new Mock<ISubmitLearnerDataClient>();
            _logger = new Mock<ILogger<GetApprenticesNiNumbersMediatorHandler>>();
        }

        [Fact]
        public async Task Then_The_SubmitLearnerDataClient_Is_Called()
        {
            //Arrange
            var request = new GetApprenticesNiNumberMediatorRequest(new List<ApprenticeEmploymentCheckModel>());

            _submitLearnerDataClient.Setup(x => x.GetApprenticesNiNumber(request.ApprenticeEmploymentCheck))
                .ReturnsAsync(new List<ApprenticeNiNumber>());

            var sut = new GetApprenticesNiNumbersMediatorHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            await sut.Handle(new GetApprenticesNiNumberMediatorRequest(new List<ApprenticeEmploymentCheckModel>()), CancellationToken.None);

            //Assert

            _submitLearnerDataClient.Verify(x => x.GetApprenticesNiNumber(request.ApprenticeEmploymentCheck), Times.Exactly(1));
        }

        [Fact]
        public async Task And_The_SubmitLearnerDataClient_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            //Arrange
            var request = new GetApprenticesNiNumberMediatorRequest(new List<ApprenticeEmploymentCheckModel>());

            _submitLearnerDataClient.Setup(x => x.GetApprenticesNiNumber(request.ApprenticeEmploymentCheck))
                .ReturnsAsync((List<ApprenticeNiNumber>)null);

            var sut = new GetApprenticesNiNumbersMediatorHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesNiNumberMediatorRequest(new List<ApprenticeEmploymentCheckModel>()), CancellationToken.None);

            //Assert

            result.ApprenticesNiNumber.Should().BeEquivalentTo(new List<ApprenticeNiNumber>());
        }

        [Fact]
        public async Task And_The_SubmitLearnerDataClient_Returns_ApprenticeNiNumbers_Then_They_Are_Returned()
        {
            //Arrange
            var request = new GetApprenticesNiNumberMediatorRequest(new List<ApprenticeEmploymentCheckModel>());

            var niNumber = new ApprenticeNiNumber(1000001, "1000001");
            var niNumbers = new List<ApprenticeNiNumber> { niNumber };

            _submitLearnerDataClient.Setup(x => x.GetApprenticesNiNumber(request.ApprenticeEmploymentCheck))
                .ReturnsAsync(niNumbers);

            var sut = new GetApprenticesNiNumbersMediatorHandler(_submitLearnerDataClient.Object, _logger.Object);

            //Act

            var result = await sut.Handle(new GetApprenticesNiNumberMediatorRequest(new List<ApprenticeEmploymentCheckModel>()), CancellationToken.None);

            //Assert

            result.ApprenticesNiNumber.Should().BeEquivalentTo(niNumbers);
        }
    }
}