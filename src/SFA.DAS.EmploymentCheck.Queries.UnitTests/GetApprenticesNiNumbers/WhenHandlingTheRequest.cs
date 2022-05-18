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
using SFA.DAS.EmploymentCheck.Queries.GetNiNumber;

namespace SFA.DAS.EmploymentCheck.Queries.UnitTests.GetApprenticesNiNumbers
{
    public class WhenHandlingTheRequest
    {
        private GetNiNumberQueryHandler _sut;
        
        private Mock<ILearnerService> _serviceMock;
        private Mock<ILogger<GetNiNumberQueryHandler>> _logger;
        private Fixture _fixture;
        private GetNiNumberQueryRequest _request;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<ILearnerService>();
            _logger = new Mock<ILogger<GetNiNumberQueryHandler>>();
            _request = _fixture.Create< GetNiNumberQueryRequest>();
            _sut = new GetNiNumberQueryHandler(_serviceMock.Object, _logger.Object);
        }

        [Test]
        public async Task Then_Stored_NiNo_Is_Returned()
        {
            // Arrange
            var niNumber = _fixture.Create<LearnerNiNumber>();

            _serviceMock.Setup(x => x.GetDbNiNumber(_request.Check))
                .ReturnsAsync(niNumber);

            // Act
            var result = await _sut.Handle(_request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(x => x.GetDbNiNumber(_request.Check), Times.Exactly(1));
            _serviceMock.Verify(x => x.GetNiNumber(_request.Check), Times.Never);
            result.LearnerNiNumber.Should().BeEquivalentTo(niNumber);
        }

        [Test]
        public async Task Then_NiNo_Is_Returned_From_The_Api_When_Not_Known()
        {
            // Arrange
            var niNumber = _fixture.Create<LearnerNiNumber>();

            _serviceMock.Setup(x => x.GetDbNiNumber(_request.Check))
                .ReturnsAsync((LearnerNiNumber)null);

            _serviceMock.Setup(x => x.GetNiNumber(_request.Check))
                .ReturnsAsync(niNumber);

            // Act
            var result = await _sut.Handle(_request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(x => x.GetDbNiNumber(_request.Check), Times.Exactly(1));
            _serviceMock.Verify(x => x.GetNiNumber(_request.Check), Times.Exactly(1));
            result.LearnerNiNumber.Should().BeEquivalentTo(niNumber);
        }

        [Test]
        public async Task Then_NiNo_Is_Returned_From_The_Api_When_NiNumber_IsNull()
        {
            // Arrange
            LearnerNiNumber niNumber = null;

            _serviceMock.Setup(x => x.GetDbNiNumber(_request.Check))
                .ReturnsAsync((LearnerNiNumber)null);

            _serviceMock.Setup(x => x.GetNiNumber(_request.Check))
                .ReturnsAsync(niNumber);

            // Act
            var result = await _sut.Handle(_request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(x => x.GetDbNiNumber(_request.Check), Times.Exactly(1));
            _serviceMock.Verify(x => x.GetNiNumber(_request.Check), Times.Exactly(1));
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
            var request = new GetNiNumberQueryRequest(null);

            // Act
            Func<Task> act = async () => await _sut.Handle(request, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }
    }
}