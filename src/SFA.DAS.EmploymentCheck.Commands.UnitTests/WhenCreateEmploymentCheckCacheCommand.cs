using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests
{
    public class WhenCreateEmploymentCheckCacheCommand_Fixture
    {
        private CreateEmploymentCheckCacheRequestCommandHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>();
            _sut = new CreateEmploymentCheckCacheRequestCommandHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var request = _fixture.Create<CreateEmploymentCheckCacheRequestCommand>();

            // Act
            await _sut.Handle(request, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.CreateEmploymentCheckCacheRequests(request.EmploymentCheckData), Times.Once);
        }
    }
}
