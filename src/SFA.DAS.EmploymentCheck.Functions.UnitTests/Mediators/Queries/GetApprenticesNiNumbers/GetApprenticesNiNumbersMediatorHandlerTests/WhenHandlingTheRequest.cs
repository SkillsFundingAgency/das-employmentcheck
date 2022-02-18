using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumber;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Mediators.Queries.GetApprenticesNiNumbers.GetApprenticesNiNumbersMediatorHandlerTests
{
    public class WhenHandlingTheRequest
    {
        private Mock<ILearnerClient> _submitLearnerDataClient;
        private Mock<ILogger<GetNiNumberQueryHandler>> _logger;
        private Fixture _fixture;
        private GetNiNumberQueryRequest _request;
        private GetNiNumberQueryHandler _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _submitLearnerDataClient = new Mock<ILearnerClient>();
            _logger = new Mock<ILogger<GetNiNumberQueryHandler>>();
            _request = new GetNiNumberQueryRequest(_fixture.Create<Functions.Application.Models.EmploymentCheck>());
            _sut = new GetNiNumberQueryHandler(_submitLearnerDataClient.Object, _logger.Object);
        }

        [Test]
        public async Task Then_The_SubmitLearnerDataClient_Is_Called()
        {
            // Arrange
            var niNumber = _fixture.Create<LearnerNiNumber>();
            _submitLearnerDataClient.Setup(x => x.GetNiNumber(_request.Check))
                .ReturnsAsync(niNumber);

            // Act
            var result = await _sut.Handle(_request, CancellationToken.None);

            // Assert
            _submitLearnerDataClient.Verify(x => x.GetNiNumber(_request.Check), Times.Exactly(1));
            result.LearnerNiNumber.Should().BeEquivalentTo(niNumber);
        }
    }
}