using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumbers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Mediators.Queries.GetDbLearnerNiNumbers.GetDbLearnerNiNumberMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private Fixture _fixture;
        private Mock<ILearnerService> _learnerService;
        private Mock<ILogger<GetDbNiNumbersQueryHandler>> _logger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _learnerService = new Mock<ILearnerService>();
            _logger = new Mock<ILogger<GetDbNiNumbersQueryHandler>>();
        }

        [Test]
        public async Task Then_The_LearnerService_Is_Called()
        {
            // Arrange
            var request = new GetDbNiNumbersQueryRequest(new List<Models.EmploymentCheck>());

            _learnerService
                .Setup(l => l.GetDbNiNumbers(request.EmploymentCheckBatch))
                .ReturnsAsync(new List<LearnerNiNumber>());

            var sut = new GetDbNiNumbersQueryHandler(_learnerService.Object, _logger.Object);

            // Act
            await sut.Handle(new GetDbNiNumbersQueryRequest(new List<Models.EmploymentCheck>()), CancellationToken.None);

            // Assert
            _learnerService.Verify(h => h.GetDbNiNumbers(request.EmploymentCheckBatch), Times.Exactly(1));
        }

        [Test]
        public async Task And_The_LearnerService_Returns_Null_Then_An_Empty_List_Is_Returned()
        {
            // Arrange
            var request = new GetDbNiNumbersQueryRequest(new List<Models.EmploymentCheck>());

            _learnerService
                .Setup(l => l.GetDbNiNumbers(request.EmploymentCheckBatch))
                .ReturnsAsync((List<LearnerNiNumber>)null);

            var sut = new GetDbNiNumbersQueryHandler(_learnerService.Object, _logger.Object);

            // Act
            var result = await sut.Handle(new GetDbNiNumbersQueryRequest(new List<Models.EmploymentCheck>()), CancellationToken.None);

            // Assert
            result.LearnerNiNumber.Should().BeEquivalentTo(new List<LearnerNiNumber>());
        }

        [Test]
        public async Task And_The_LearnerServicet_Returns_LearnerNiNumbers_Then_They_Are_Returned()
        {
            // Arrange
            var request = new GetDbNiNumbersQueryRequest(new List<Models.EmploymentCheck>());
            var niNumbers = _fixture.CreateMany<LearnerNiNumber>(1).ToList();

            _learnerService
                .Setup(x => x.GetDbNiNumbers(request.EmploymentCheckBatch))
                .ReturnsAsync(niNumbers);

            var sut = new GetDbNiNumbersQueryHandler(_learnerService.Object, _logger.Object);

            // Act
            var result = await sut.Handle(new GetDbNiNumbersQueryRequest(new List<Models.EmploymentCheck>()), CancellationToken.None);

            // Assert
            result.LearnerNiNumber.Should().BeEquivalentTo(niNumbers);
        }
    }
}