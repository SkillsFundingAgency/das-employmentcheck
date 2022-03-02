using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumber;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetDbLearnerNiNumbers.GetDbLearnerNiNumberMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<ILearnerService> _learnerService;
        private Mock<ILogger<GetDbNiNumberQueryHandler>> _logger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _learnerService = new Mock<ILearnerService>();
            _logger = new Mock<ILogger<GetDbNiNumberQueryHandler>>();
        }

        [Test]
        public async Task Then_The_LearnerService_Is_Called()
        {
            // Arrange
            var request = _fixture.Create<GetDbNiNumberQueryRequest>();

            _learnerService
                .Setup(l => l.GetDbNiNumber(request.EmploymentCheck))
                .ReturnsAsync(new LearnerNiNumber());

            var sut = new GetDbNiNumberQueryHandler(_learnerService.Object, _logger.Object);

            // Act
            await sut.Handle(request, CancellationToken.None);

            // Assert
            _learnerService.Verify(h => h.GetDbNiNumber(request.EmploymentCheck), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_LearnerService_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            var request = new GetDbNiNumberQueryRequest(new Models.EmploymentCheck());

            _learnerService
                .Setup(l => l.GetDbNiNumber(request.EmploymentCheck))
                .ReturnsAsync(() => null);

            var sut = new GetDbNiNumberQueryHandler(_learnerService.Object, _logger.Object);

            // Act
            var result = await sut.Handle(new GetDbNiNumberQueryRequest(new Models.EmploymentCheck()), CancellationToken.None);

            // Assert
            result.LearnerNiNumber.Should().BeEquivalentTo(new LearnerNiNumber());
        }

        [Test]
        public async Task And_The_LearnerServicet_Returns_A_LearnerNiNumber_Then_It_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetDbNiNumberQueryRequest>();
            var niNumber = _fixture.Create<LearnerNiNumber>();

            _learnerService
                .Setup(x => x.GetDbNiNumber(request.EmploymentCheck))
                .ReturnsAsync(niNumber);

            var sut = new GetDbNiNumberQueryHandler(_learnerService.Object, _logger.Object);

            // Act
            var result = await sut.Handle(request, CancellationToken.None);

            // Assert
            result.LearnerNiNumber.Should().BeEquivalentTo(niNumber);
        }
    }
}