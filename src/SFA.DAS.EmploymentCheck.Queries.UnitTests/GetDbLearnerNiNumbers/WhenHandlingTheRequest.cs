using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetDbLearnerNiNumbers
{
    public class WhenHandlingTheRequest
    {
        private GetDbNiNumberQueryHandler _sut;
        private Fixture _fixture;
        private Mock<ILearnerService> _learnerService;
        private Mock<ILogger<GetDbNiNumberQueryHandler>> _logger;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _learnerService = new Mock<ILearnerService>();
            _logger = new Mock<ILogger<GetDbNiNumberQueryHandler>>();

            _sut = new GetDbNiNumberQueryHandler(_learnerService.Object, _logger.Object);
        }

        [Test]
        public async Task Then_The_LearnerService_Is_Called()
        {
            // Arrange
            var request = _fixture.Create<GetDbNiNumberQueryRequest>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _learnerService.Verify(h => h.GetDbNiNumber(request.EmploymentCheck), Times.Exactly(1));
        }

        [Test]
        public async Task Then_The_LearnerService_Returns_Null_Then_Null_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetDbNiNumberQueryRequest>();

            _learnerService
                .Setup(l => l.GetDbNiNumber(request.EmploymentCheck))
                .ReturnsAsync((LearnerNiNumber)null);

            // Act
            var result = await _sut.Handle(new GetDbNiNumberQueryRequest(new Data.Models.EmploymentCheck()), CancellationToken.None);

            // Assert
            result.LearnerNiNumber.Should().BeNull();
        }

        [Test]
        public async Task Then_The_LearnerService_Returns_A_LearnerNiNumber_Then_It_Is_Returned()
        {
            // Arrange
            var request = _fixture.Create<GetDbNiNumberQueryRequest>();
            var niNumber = _fixture.Create<LearnerNiNumber>();

            _learnerService
                .Setup(x => x.GetDbNiNumber(request.EmploymentCheck))
                .ReturnsAsync(niNumber);

            // Act
            var result = await _sut.Handle(request, CancellationToken.None);

            // Assert
            result.LearnerNiNumber.Should().BeEquivalentTo(niNumber);
        }

        [Test]
        public async Task Then_ArgumentException_Is_Thrown_When_Request_Is_Null()
        {
            // Act
            Func<Task> act = async () => await _sut.Handle(null, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Test]
        public async Task Then_ArgumentException_Is_Thrown_When_EmploymentCheck_Is_Null()
        {
            // Arrange
            var request = new GetDbNiNumberQueryRequest(null);

            // Act
            Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}