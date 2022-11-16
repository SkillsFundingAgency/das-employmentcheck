using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Commands.AbandonRelatedRequests;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Domain.Enums;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests.AbandonRelatedRequests
{
    public class WhenHandlingTheRequest
    {
        private AbandonRelatedRequestsCommandHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>();
            _sut = new AbandonRelatedRequestsCommandHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<AbandonRelatedRequestsCommand>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.AbandonRelatedRequests(request.EmploymentCheckCacheRequests), Times.Once);
        }
    }
}
