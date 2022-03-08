using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Commands.UnitTests
{
    public class WhenStoreCompletedEmploymentCheckCacheCommandHandler
    {
        private StoreCompletedEmploymentCheckCommandHandler _sut;
        private Mock<IEmploymentCheckService> _serviceMock;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _serviceMock = new Mock<IEmploymentCheckService>();
            _sut = new StoreCompletedEmploymentCheckCommandHandler(_serviceMock.Object);
        }

        [Test]
        public async Task Then_Service_is_called()
        {
            // Arrange
            var employmentCheck = _fixture.Create<Models.EmploymentCheck>();
            var cmd = new StoreCompletedEmploymentCheckCommand(employmentCheck);

            _serviceMock
                .Setup(x => x.StoreCompletedCheck(employmentCheck))
                .Verifiable();

            // Act
            var result = await _sut.Handle(cmd, CancellationToken.None);

            // Assert
            _serviceMock.Verify(s => s.StoreCompletedCheck(cmd.EmploymentCheck), Times.Once);
            result.Should().Be(MediatR.Unit.Value);
        }
    }
}
